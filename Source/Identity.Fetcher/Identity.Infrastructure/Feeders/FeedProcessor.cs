using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Identity.Domain;
using Identity.Domain.Events;
using Identity.Infrastructure.Repositories;
using MoreLinq;

namespace Identity.Infrastructure.Feeders
{
    public class FeedProcessor
    {
        private readonly ILogger log;
        private readonly ChannelRepository channelRepo;
        private readonly UserRepository userRepo;
        private readonly PostRepository postRepo;
        private readonly PostNlpAnalyzer nlpAnalyzer;

        private readonly long[] FeedersWithTagsAsUpstreams = new[] { 1L, 2L, 4L, 5L, 6L, 7L, 14L, 25L, 1000L };

        public FeedProcessor(ILogger log, ChannelRepository channelRepo, UserRepository userRepo, PostRepository postRepo, PostNlpAnalyzer nlpAnalyzer)
        {
            this.log = log;
            this.channelRepo = channelRepo;
            this.userRepo = userRepo;
            this.postRepo = postRepo;
            this.nlpAnalyzer = nlpAnalyzer;
        }

        public void ProcessFeed(User rssFeederUser, Feed feed, IChannelLinkEventListener eventListener, IEnumerable<FeedItem> items)
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
                        eventListener.Add(new ChannelAdded { ChannelId = newChannel.Id });
                        channelRepo.AddSubscription(feed.ChannelId, newChannel.Id);
                        eventListener.Add(new SubscriptionAdded { UpstreamChannelId = newChannel.Id, DownstreamChannelId = feed.ChannelId });
                        allUpstreamChannels[newChannel.Name] = newChannel.Id;
                    }
                }
            }

            feed.LastFetch = DateTimeOffset.Now;
            channelRepo.UpdateFeed(feed);

            var newPosts = new List<Post>();

            foreach (var feedItem in items)
            {
                try
                {
                    if (!postRepo.SimilarPostAlreadyExists(feedItem.Title, feedItem.CreatedAt, feed.ChannelId) && feedItem.Links.Any())
                    {                        
                        log.Info("processing item " + feedItem.Title + " from feed " + feed.Url);

                        var url = feedItem.Links.First().ToString();
                        var post = postRepo.GetByUrl(url);

                        if (post == null)
                        {
                            post = AddNewPost(feedItem);
                            newPosts.Add(post);
                        }

                        if (FeedersWithTagsAsUpstreams.Contains(feed.Id))
                        {
                            foreach (var tag in feedItem.Tags)
                            {
                                userRepo.Publish(rssFeederUser.Id, allUpstreamChannels[tag], post.Id);
                                eventListener.Add(new PostAdded { ChannelId = allUpstreamChannels[tag], PostId = post.Id });
                            }
                        }

                        userRepo.Publish(rssFeederUser.Id, feed.ChannelId, post.Id);
                        eventListener.Add(new PostAdded { ChannelId = feed.ChannelId, PostId = post.Id });
                    }
                        
                }
                catch (Exception ex)
                {
                    log.Error("Processing of feed item " + feedItem.Title + " failed", ex);
                }
            }

            if (feed.Id == 36 || feed.Id == 1000)
            {
                nlpAnalyzer.AnalyzePosts(newPosts);
            }            
        }

        private Post AddNewPost(FeedItem feedItem)
        {
            var url = feedItem.Links.First().ToString();

            var post = new Post
            {
                Created = feedItem.CreatedAt,
                Description = feedItem.Content,
                Title = feedItem.Title,
                Uri = url,
                PremiumContent = url.Contains("protected/premium")
            };

            postRepo.AddPost(post, false); 
            postRepo.TagPost(post.Id, feedItem.Tags);

            return post;
        }
    }
}