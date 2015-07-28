using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using log4net;

namespace Identity.Fetcher.WindowsService.Rss
{
    class RssFeedRefresher
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChannelRepository channelRepo;
        readonly PostRepository postRepo;
        readonly UserRepository userRepo;
        private const string rssFeederUsername = "rssfeeder";

        public RssFeedRefresher(PostRepository postRepo, UserRepository userRepo, ChannelRepository channelRepo)
        {
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
        }

        public void Run()
        {
            var rssFeederUser = userRepo.FindByName(rssFeederUsername);

            var fetchRssFeedersBlock = new TransformBlock<RssFeeder, FeedX>(rssFeeder => new FeedX
            {
                Url = rssFeeder.Url,
                ChannelIds = channelRepo.GetChannelsForRssFeeder(rssFeeder.Id).ToList(),
                Tags = channelRepo.GetRssFeederTags(rssFeeder.Id)
            });

            var fetchFeedItemsBlock = new TransformManyBlock<FeedX, Tuple<FeedX, SyndicationItem>>(f =>
            {
                log.Info("fetching feed " + f.Url);
                try
                {
                    var rssReader = new RssReader(f.Url);
                    var feed = rssReader.ReadRss();
                    return feed.Items.Select(i => new Tuple<FeedX, SyndicationItem>(f, i));
                }
                catch (Exception ex)
                {
                    log.Error("Unable to fetch feed " + f.Url, ex);
                    return new Tuple<FeedX, SyndicationItem>[0];
                }

            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });
            
            var storeFeedItemBlock = new ActionBlock<Tuple<FeedX, SyndicationItem>>(t =>
            {
                try
                {
                    var url = t.Item2.Links.First().Uri.ToString();
                    
                    if (!postRepo.PostExists(url))
                    {
                        log.Info("processing item " + t.Item2.Title.Text + " from feed " + t.Item1.Url);

                        var post = new Post
                        {
                            Created = GetCreatedTime(t.Item2),
                            Description = GetDescription(t.Item2),
                            Title = t.Item2.Title.Text,
                            Uri = url,
                        };

                        postRepo.AddPost(post);

                        postRepo.TagPost(post.Id, t.Item2.Categories.Select(c => c.Name).Union(t.Item1.Tags));

                        foreach (var channelId in t.Item1.ChannelIds)
                        {
                            userRepo.Publish(rssFeederUser.Id, channelId, post.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Unable store item " + t.Item2.Title.Text, ex);
                }      
            });

            fetchRssFeedersBlock.LinkTo(fetchFeedItemsBlock);
            //fetchFeedItemsBlock.LinkTo(storeFeedItemBlock);

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


            var feeders = channelRepo.AllRssFeeders();

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
        public string Url { get; set; }

        public IEnumerable<long> ChannelIds { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
