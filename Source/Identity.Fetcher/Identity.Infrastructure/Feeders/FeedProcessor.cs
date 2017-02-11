using System;
using System.Collections.Generic;
using System.Linq;
using Identity.Domain;
using Identity.Domain.Events;
using Identity.Infrastructure.Repositories;
using MoreLinq;

namespace Identity.Infrastructure.Feeders
{
    class FeedProcessor
    {
        private readonly Logger log;
        private readonly ChannelRepository channelRepo;
        private readonly UserRepository userRepo;
        private readonly PostRepository postRepo;

        private readonly long[] FeedersWithTagsAsUpstreams = new[] { 1L, 2L, 4L, 5L, 6L, 7L };

        public FeedProcessor(Logger log, ChannelRepository channelRepo, UserRepository userRepo, PostRepository postRepo)
        {
            this.log = log;
            this.channelRepo = channelRepo;
            this.userRepo = userRepo;
            this.postRepo = postRepo;
        }

        public void ProcessFeed(User rssFeederUser, Feed feed, IList<IChannelLinkEvent> events, IEnumerable<FeedItem> items)
        {
            log.Info("Processing feed items from feed " + feed.Url);

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

            foreach (var feedItem in items)
            {
                try
                {
                    ProcessFeedItem(feed, rssFeederUser, feedItem, allUpstreamChannels, events);
                }
                catch (Exception ex)
                {
                    log.Error("Processing of feed item " + feedItem.Title + " failed", ex);
                }
            }
        }

        private void ProcessFeedItem(Feed feed, User rssFeederUser, FeedItem feedItem, IDictionary<string, long> upstreamChannels, IList<IChannelLinkEvent> events)
        {
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
                        events.Add(new PostAdded { ChannelId = upstreamChannels[tag], PostId = post.Id });
                    }
                }

                postRepo.TagPost(post.Id, feedItem.Tags);
            }

            userRepo.Publish(rssFeederUser.Id, feed.ChannelId, post.Id);
            events.Add(new PostAdded { ChannelId = feed.ChannelId, PostId = post.Id });
        }
    }
}