using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Identity.Domain;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using log4net;
using Post = Identity.Domain.Post;
using User = Identity.Domain.User;

namespace Identity.Infrastructure.Services
{
    public class PostListLoader
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PostRepository postRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly ChannelRepository channelRepo;
        private readonly UserRepository userRepo;
        private readonly IEnumerable<InlineArticleSelector> inlineArticleSelectors;

        public PostListLoader(ChannelRepository channelRepo, CommentRepostitory commentRepo, PostRepository postRepo, InlineArticleSelectorRepository inlineArticleSelectorRepo, UserRepository userRepo)
        {
            this.channelRepo = channelRepo;
            this.commentRepo = commentRepo;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            this.inlineArticleSelectors = inlineArticleSelectorRepo.GetAll();
        }

        public IList<DTO.Post> Load(long id, User user, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy, int pageSize)
        {
            log.Debug("Fetching items from channel " + id + " and page " + fromIndex);

            var channel = channelRepo.GetById(id);

            if (channel == null)
            {
                return null;
            }

            var posts = onlyUnread ? 
                postRepo.UnreadPostsFromChannel(user.Id, channel.Id, orderBy, pageSize).ToList() : 
                postRepo.PostsFromChannel(user.Id, channel.Id, fromIndex, orderBy, pageSize).ToList();

            if (!user.IsPremium)
            {
                posts = posts.Select(p => p.PremiumContent ? Post.Empty : p).ToList();
            }

            var postIds = posts.Select(p => p.Id).ToList();
            var allTags = postRepo.Tags(postIds).ToList();
            var blockedTags = userRepo.BlockedTagIds(user.Id);
            var commentCounts = commentRepo.CommentCount(postIds).ToList();
            var publishedIns = postRepo.PublishedIn(postIds, user.Id).ToList();
            var result = posts
                .Where(p => !allTags.Where(t => t.PostId == p.Id).Select(t => t.TagId).Intersect(blockedTags).Any())
                .Select(Mapper.Map<DTO.Post>).ToList();
            foreach (var p in result)
            {
                p.Tags = allTags.Where(t => t.PostId == p.Id).Select(t => t.Tag).ToList();
                p.CommentCount = commentCounts.Any(c => c.PostId == p.Id) ? commentCounts.Single(c => c.PostId == p.Id).Count : 0;
                p.IsCollapsed = true; //p.Description.Length >= 500; 
                p.PublishedIn = publishedIns
                    .Where(pi => pi.PostId == p.Id)
                    .OrderByDescending(pi => pi.Count)
                    .Take(5)
                    .Select(pi => new ChannelReference
                    {
                        Id = pi.ChannelId,
                        Name = pi.ChannelName
                    })
                    .ToList();
                p.CanBeInlined = inlineArticleSelectors.Any(s => p.Uri.Contains(s.UrlPattern));
            }
            log.Debug("Posts loaded");
            //log.Debug("Items from channel [" + String.Join(",", result.Select(r => r.Id)) + "] was fetched");

            return result;            
        }

 
    }
}