using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using Identity.Domain;
using Identity.Infrastructure.Feeders;
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
                var channelRepo = new ChannelRepository(session.Transaction);
                var userRepo = new UserRepository(session.Transaction);

                rssFeederUser = userRepo.FindByName(rssFeederUsername);
                feeders = channelRepo.OutOfSyncRssFeeders(TimeSpan.FromHours(1));
            }

            if (rssFeederUser == null)
            {
                log.Error("User 'rssFeeder' was not found");
                return;
            }

            var fetchFeedItemsBlock = new TransformBlock<RssFeeder, Tuple<RssFeeder, IEnumerable<FeedItem>>>(rssFeeder =>
            {
                log.Info("fetching feed " + rssFeeder.Url);
                try
                {
                    var rssReader = GetReader(rssFeeder);
                    var feed = rssReader.Fetch();
                    return new Tuple<RssFeeder, IEnumerable<FeedItem>>(rssFeeder, feed);
                }
                catch (Exception ex)
                {
                    log.Error("Unable to fetch feed " + rssFeeder.Url, ex);
                    return new Tuple<RssFeeder, IEnumerable<FeedItem>>(rssFeeder, new List<FeedItem>());
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });

            var storeFeedItemBlock = new ActionBlock<Tuple<RssFeeder, IEnumerable<FeedItem>>>(t =>
            {
                if (!t.Item2.Any())
                {
                    return;
                }

                using (var session = connectionFactory.NewTransaction())
                {
                    var channelRepo = new ChannelRepository(session.Transaction);
                    var postRepo = new PostRepository(session.Transaction);
                    var userRepo = new UserRepository(session.Transaction);
                    //var autoTagger = new AutoTagger(new TagCountRepository(session), postRepo);

                    log.Info("Processing feed items from feed " + t.Item1.Url);

                    var channelIds = channelRepo.GetChannelsForRssFeeder(t.Item1.Id).ToList();

                    if (!channelIds.Any())
                    {
                        return;
                    }

                    var tags = channelRepo.GetRssFeederTags(t.Item1.Id);

                    t.Item1.LastFetch = DateTimeOffset.Now;
                    channelRepo.UpdateRssFeeder(t.Item1);

                    try
                    {
                        foreach (var feedItem in t.Item2)
                        {
                            if (postRepo.FeedItemAlreadyPosted(feedItem.Title, feedItem.CreatedAt, t.Item1.Id))
                                break;

                            var url = feedItem.Links.First().ToString();
                            var post = postRepo.GetByUrl(url);

                            if (post == null)
                            {
                                log.Info("processing item " + feedItem.Title + " from feed " + t.Item1.Url);

                                post = new Post
                                {
                                    Created = feedItem.CreatedAt,
                                    Description = feedItem.Content,
                                    Title = feedItem.Title,
                                    Uri = url,
                                };

                                postRepo.AddPost(post, true);
                                postRepo.TagPost(post.Id, feedItem.Tags.Union(tags));
                                //autoTagger.AutoTag(post);
                            }

                            postRepo.AddFeedItem(t.Item1.Id, post.Id, feedItem.CreatedAt);

                            foreach (var channelId in channelIds)
                            {
                                userRepo.Publish(rssFeederUser.Id, channelId, post.Id);
                            }
                        }

                        session.Commit();

                        log.Info("Done processing feed items from feed " + t.Item1.Url);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Unable store feed items from feed " + t.Item1.Url, ex);
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });

            fetchFeedItemsBlock.LinkTo(storeFeedItemBlock);
            
            fetchFeedItemsBlock.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)storeFeedItemBlock).Fault(t.Exception);
                else storeFeedItemBlock.Complete();
            });
   
            foreach (var feeder in feeders)
            {
                fetchFeedItemsBlock.Post(feeder);
            }

            fetchFeedItemsBlock.Complete();

            storeFeedItemBlock.Completion.Wait();
        }

        private IFeederReader GetReader(RssFeeder feeder)
        {
            switch (feeder.Type)
            {
                case FeederType.Rss:
                    return new RssReader(feeder.Url);
                case FeederType.Twitter:
                    return new TwitterFeeder(feeder.Url);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
