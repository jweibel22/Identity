using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using log4net;
using User = Identity.Domain.User;

namespace Identity.Infrastructure.Services
{
    public class PostListLoader
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly PostRepository postRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly ChannelRepository channelRepo;

        public PostListLoader(ChannelRepository channelRepo, CommentRepostitory commentRepo, PostRepository postRepo)
        {
            this.channelRepo = channelRepo;
            this.commentRepo = commentRepo;
            this.postRepo = postRepo;
        }

        public IList<DTO.Post> Load(long id, User user, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy, int pageSize)
        {
            log.Debug("Fetching items from channel " + id + " and page " + fromIndex);

            var channel = channelRepo.GetById(id);

            if (channel == null)
            {
                return null;
            }

            var posts = postRepo.PostsFromChannel(user.Id, onlyUnread, channel.Id, timestamp, 0, orderBy, pageSize).ToList();
            var postIds = posts.Select(p => p.Id).ToList();
            var allTags = postRepo.Tags(postIds).ToList();
            var commentCounts = commentRepo.CommentCount(postIds).ToList();
            var publishedIns = postRepo.PublishedIn(postIds, user.Id).ToList();
            var result = posts.Select(Mapper.Map<DTO.Post>).ToList();
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
            }
            log.Debug("Posts loaded");
            //log.Debug("Items from channel [" + String.Join(",", result.Select(r => r.Id)) + "] was fetched");

            return result;            
        }

 
    }
}