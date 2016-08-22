using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using log4net;


namespace Identity.Infrastructure.Rss
{
    public class RssFeedRefresher
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConnectionFactory connectionFactory;
        private const string rssFeederUsername = "rssfeeder";

        public RssFeedRefresher(ConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public void Run()
        {
            User rssFeederUser;
            IEnumerable<RssFeeder> feeders;

            using (var session = connectionFactory.NewTransaction())
            {
                var channelRepo = new ChannelRepository(session);
                var userRepo = new UserRepository(session);

                rssFeederUser = userRepo.FindByName(rssFeederUsername);
                feeders = channelRepo.OutOfSyncRssFeeders(TimeSpan.FromHours(1));
            }

            if (rssFeederUser == null)
            {
                log.Error("User 'rssFeeder' was not found");
                return;
            }

            var fetchRssFeedersBlock = new TransformBlock<Identity.Domain.RssFeeder, FeedX>(rssFeeder =>
            {
                using (var session = connectionFactory.NewTransaction())
                {
                    var channelRepo = new ChannelRepository(session);
                    return new FeedX
                    {
                        RssFeeder = rssFeeder,
                        ChannelIds = channelRepo.GetChannelsForRssFeeder(rssFeeder.Id).ToList(),
                        Tags = channelRepo.GetRssFeederTags(rssFeeder.Id)
                    };
                }
            });

            var fetchFeedItemsBlock = new TransformManyBlock<FeedX, Tuple<FeedX, SyndicationItem>>(f =>
            {
                using (var session = connectionFactory.NewTransaction())
                {
                    log.Info("fetching feed " + f.RssFeeder.Url);
                    var channelRepo = new ChannelRepository(session);
                    try
                    {
                        var rssReader = new RssReader(f.RssFeeder.Url);
                        var feed = rssReader.ReadRss();

                        f.RssFeeder.LastFetch = DateTimeOffset.Now;
                        channelRepo.UpdateRssFeeder(f.RssFeeder);

                        session.Commit();
                        return feed.Items.Select(i => new Tuple<FeedX, SyndicationItem>(f, i));                        
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unable to fetch feed " + f.RssFeeder.Url, ex);
                        return new Tuple<FeedX, SyndicationItem>[0];
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });
            
            var storeFeedItemBlock = new ActionBlock<Tuple<FeedX, SyndicationItem>>(t =>
            {
                using (var session = connectionFactory.NewTransaction())
                {
                    var channelRepo = new ChannelRepository(session);
                    var postRepo = new PostRepository(session);
                    var userRepo = new UserRepository(session);
                    //var autoTagger = new AutoTagger(new TagCountRepository(session), postRepo);

                    try
                    {
                        var url = t.Item2.Links.First().Uri.ToString();
                        var created = GetCreatedTime(t.Item2);
                        var title = t.Item2.Title.Text;


                        if (postRepo.FeedItemAlreadyPosted(title, created, t.Item1.RssFeeder.Id)) return;

                        var post = postRepo.GetByUrl(url);

                        if (post == null)
                        {
                            //log.Info("processing item " + t.Item2.Title.Text + " from feed " + t.Item1.RssFeeder.Url);

                            post = new Post
                            {
                                Created = created,
                                Description = GetDescription(t.Item2),
                                Title = title,
                                Uri = url,
                            };

                            postRepo.AddPost(post, true);
                            postRepo.TagPost(post.Id, t.Item2.Categories.Select(c => c.Name).Union(t.Item1.Tags));
                            //autoTagger.AutoTag(post);
                        }

                        //TODO: we should probably add this feeds tags to the post

                        postRepo.AddFeedItem(t.Item1.RssFeeder.Id, post.Id, created);

                        foreach (var channelId in t.Item1.ChannelIds)
                        {
                            userRepo.Publish(rssFeederUser.Id, channelId, post.Id);
                        }

                        session.Commit();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unable store item " + t.Item2.Title.Text, ex);
                    }
                }
            });

            fetchRssFeedersBlock.LinkTo(fetchFeedItemsBlock);
            fetchFeedItemsBlock.LinkTo(storeFeedItemBlock);

            fetchRssFeedersBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)fetchFeedItemsBlock).Fault(t.Exception);
                else fetchFeedItemsBlock.Complete();
            });
            fetchFeedItemsBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)storeFeedItemBlock).Fault(t.Exception);
                else storeFeedItemBlock.Complete();
            });


            foreach (var feeder in feeders)
            {
                fetchRssFeedersBlock.Post(feeder);
            }

            fetchRssFeedersBlock.Complete();

            storeFeedItemBlock.Completion.Wait();
        }

        private DateTime GetCreatedTime(SyndicationItem item)
        {
            if (item.PublishDate != DateTimeOffset.MinValue)
            {
                return item.PublishDate.ToUniversalTime().DateTime;
            }
            else if (item.LastUpdatedTime != DateTimeOffset.MinValue)
            {
                return item.LastUpdatedTime.ToUniversalTime().DateTime;
            }
            else
            {
                return DateTime.UtcNow;
            }
        }

        private string ReadContent(SyndicationItem item)
        {
            var contents =
                from extension in item.ElementExtensions
                select extension.GetObject<XElement>() into ele
                where ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content")
                select ele.Value;

            return contents.FirstOrDefault();
        }

        private string GetDescription(SyndicationItem item)
        {
            var result = ReadContent(item);

            if (result != null)
                return result;

            return item.Summary == null ? "" : item.Summary.Text;
        }
    }

    class FeedX
    {
        public Domain.RssFeeder RssFeeder { get; set; }

        public IEnumerable<long> ChannelIds { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
