using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using Identity.Domain;
using Identity.Infrastructure.Feeders;
using Identity.Infrastructure.Repositories;
using log4net;


namespace Identity.Infrastructure.Rss
{
    class Logger
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TextWriter azureLog;

        public Logger(TextWriter azureLog)
        {
            this.azureLog = azureLog;
        }

        public void Info(string message)
        {
            log.Info(message);
            azureLog.WriteLine(message);
        }

        public void Error(string message, Exception ex)
        {
            log.Error(message, ex);
            azureLog.WriteLine(String.Format("{0}. Reason: {1}", message, ex));
        }
    }

    public class RssFeedRefresher
    {
        //private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConnectionFactory connectionFactory;
        private readonly FeederFactory feederFactory;
        private readonly Logger log;

        public RssFeedRefresher(ConnectionFactory connectionFactory, TextWriter azureLog)
        {
            this.connectionFactory = connectionFactory;
            this.feederFactory = new FeederFactory();
            log = new Logger(azureLog);
        }

        private void ProcessFeed(DbSession session, User rssFeederUser, Feed feed, ChannelLinkGraph graph, IEnumerable<FeedItem> items)
        {
            var channelRepo = new ChannelRepository(session.Transaction);
            var postRepo = new PostRepository(session.Transaction);
            var userRepo = new UserRepository(session.Transaction);
            //var autoTagger = new AutoTagger(new TagCountRepository(session), postRepo);

            log.Info("Processing feed items from feed " + feed.Url);

            var tags = channelRepo.GetFeedTags(feed.Id);

            feed.LastFetch = DateTimeOffset.Now;
            channelRepo.UpdateFeed(feed);
            graph.MarkAsDirty(feed.ChannelId);

            foreach (var feedItem in items)
            {
                try
                {
                    if (postRepo.FeedItemAlreadyPosted(feedItem.Title, feedItem.CreatedAt, feed.Id))
                        break;

                    var url = feedItem.Links.First().ToString();
                    var post = postRepo.GetByUrl(url);

                    if (post == null)
                    {
                        log.Info("processing item " + feedItem.Title + " from feed " + feed.Url);

                        post = new Post
                        {
                            Created = feedItem.CreatedAt,
                            Description = feedItem.Content,
                            Title = feedItem.Title,
                            Uri = url,
                            PremiumContent = url.Contains("protected/premium")
                        };

                        postRepo.AddPost(post, false);
                        postRepo.TagPost(post.Id, feedItem.Tags.Union(tags));
                        //autoTagger.AutoTag(post);
                    }

                    postRepo.AddFeedItem(feed.Id, post.Id, feedItem.CreatedAt);

                    userRepo.Publish(rssFeederUser.Id, feed.ChannelId, post.Id);
                }
                catch (Exception ex)
                {
                    log.Error("Processing of feed item " + feedItem.Title + " failed", ex);
                }
            }
        }

        public void Run(User rssFeederUser, IEnumerable<Feed> feeders)
        {
            ChannelLinkGraph graph;

            using (var session = connectionFactory.NewTransaction())
            {
                var channelLinkRepo = new ChannelLinkRepository(session.Transaction);
                graph = channelLinkRepo.GetGraph();
            }

            var fetchFeedItemsBlock = new TransformBlock<Feed, Tuple<Feed, IEnumerable<FeedItem>>>(rssFeeder =>
            {
                log.Info("fetching feed " + rssFeeder.Url);
                try
                {
                    var rssReader = feederFactory.GetReader(rssFeeder);
                    var feed = rssReader.Fetch(rssFeeder.Url);
                    return new Tuple<Feed, IEnumerable<FeedItem>>(rssFeeder, feed);
                }
                catch (Exception ex)
                {
                    log.Error("Unable to fetch feed " + rssFeeder.Url, ex);
                    return new Tuple<Feed, IEnumerable<FeedItem>>(rssFeeder, new List<FeedItem>());
                }
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 5
            });

            var storeFeedItemBlock = new ActionBlock<Tuple<Feed, IEnumerable<FeedItem>>>(t =>
            {
                if (!t.Item2.Any())
                {
                    return;
                }

                using (var session = connectionFactory.NewTransaction())
                {
                    try
                    {
                        ProcessFeed(session, rssFeederUser, t.Item1, graph, t.Item2);   
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

            log.Info("Refreshing unread counts");

            using (var session = connectionFactory.NewTransaction())
            {
                var repo = new ChannelLinkRepository(session.Transaction);

                foreach (var edge in graph.DirtyUserChannels)
                {
                    repo.UpdateUnreadCounts(edge);
                }

                session.Commit();
            }
        }
    }

    class FeederFactory
    {
        private readonly TwitterFeeder twitter;
        private readonly RssReader rss;

        public FeederFactory()
        {
            twitter = new TwitterFeeder();
            rss = new RssReader();
        }

        public IFeederReader GetReader(Feed feeder)
        {
            switch (feeder.Type)
            {
                case FeedType.Rss:
                    return rss;
                case FeedType.Twitter:
                    return twitter;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
