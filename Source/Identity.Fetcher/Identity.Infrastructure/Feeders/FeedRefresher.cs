using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Identity.Domain;
using Identity.Infrastructure.Feeders;
using Identity.Infrastructure.Repositories;
using MoreLinq;

namespace Identity.Infrastructure.Rss
{
    public class FeedRefresher
    {
        //private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConnectionFactory connectionFactory;
        private readonly FeederFactory feederFactory;
        private readonly Logger log;

        private readonly long[] FeedersWithTagsAsUpstreams = new[] {1L, 2L, 4L, 5L, 6L, 7L};

        public FeedRefresher(ConnectionFactory connectionFactory, TextWriter azureLog)
        {
            this.connectionFactory = connectionFactory;
            this.feederFactory = new FeederFactory();
            log = new Logger(azureLog);
        }

        private void ProcessFeedItem(DbSession session, Feed feed, User rssFeederUser, FeedItem feedItem, IDictionary<string, long> upstreamChannels)
        {
            var userRepo = new UserRepository(session.Transaction);
            var postRepo = new PostRepository(session.Transaction);

            if (postRepo.SimilarPostAlreadyExists(feedItem.Title, feedItem.CreatedAt, feed.ChannelId))
                return;

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

                if (FeedersWithTagsAsUpstreams.Contains(feed.Id))
                {
                    foreach (var tag in feedItem.Tags)
                    {
                        userRepo.Publish(rssFeederUser.Id, upstreamChannels[tag], post.Id);
                    }
                }

                postRepo.TagPost(post.Id, feedItem.Tags);
            }

            userRepo.Publish(rssFeederUser.Id, feed.ChannelId, post.Id);
        }

        private void ProcessFeed(DbSession session, User rssFeederUser, Feed feed, ChannelLinkGraph graph, IEnumerable<FeedItem> items)
        {
            var channelRepo = new ChannelRepository(session.Transaction);
            //var autoTagger = new AutoTagger(new TagCountRepository(session), postRepo);
            
            log.Info("Processing feed items from feed " + feed.Url);

            //var tags = channelRepo.GetFeedTags(feed.Id);

            var existingUpstreamChannels = channelRepo.GetAllDirectUpStreamChannels(feed.ChannelId).DistinctBy(c => c.Name).ToList();
            var allUpstreamChannels = existingUpstreamChannels.ToDictionary(channel => channel.Name.Trim(), channel => channel.Id);

            if (FeedersWithTagsAsUpstreams.Contains(feed.Id))
            {
                var allTags = items.SelectMany(i => i.Tags).Distinct().Select(tag => tag.Trim());

                foreach (var tag in allTags)
                {
                    if (!allUpstreamChannels.ContainsKey(tag))
                    {
                        var newChannel = new Channel
                        {
                            Created = DateTimeOffset.Now,
                            IsPublic = true,
                            Name = tag
                        };

                        channelRepo.AddChannel(newChannel);
                        channelRepo.AddSubscription(feed.ChannelId, newChannel.Id);
                        allUpstreamChannels[newChannel.Name] = newChannel.Id;
                    }
                }
            }

            feed.LastFetch = DateTimeOffset.Now;
            channelRepo.UpdateFeed(feed);
            graph.MarkAsDirty(feed.ChannelId);

            foreach (var feedItem in items)
            {
                try
                {
                    ProcessFeedItem(session, feed, rssFeederUser, feedItem, allUpstreamChannels);
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
}
