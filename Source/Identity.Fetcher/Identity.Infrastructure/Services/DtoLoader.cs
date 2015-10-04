using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Domain;
using Identity.Infrastructure.Repositories;

namespace Identity.Infrastructure.Services
{
    public interface ILoadDtos
    {
        IEnumerable<DTO.Post> LoadPosts(User user, IEnumerable<Post> post);
        DTO.Post LoadPost(User user, Post post);

        IEnumerable<DTO.Post> LoadChannelPosts(User user, Channel channel, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy);
        DTO.Channel LoadChannel(User user, Channel channel);
        IEnumerable<DTO.Channel> LoadChannelList(User user, IEnumerable<Channel> channels);
        DTO.RssFeeder LoadRssFeeder(RssFeeder rssFeeder);
    }

    public class DtoLoader : ILoadDtos
    {
        private readonly PostRepository postRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly UserRepository userRepo;
        private readonly ChannelRepository channelRepo;

        public DtoLoader(PostRepository postRepo, CommentRepostitory commentRepo, UserRepository userRepo, ChannelRepository channelRepo)
        {
            this.postRepo = postRepo;
            this.commentRepo = commentRepo;
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
        }

        public IEnumerable<DTO.Post> LoadPosts(User user, IEnumerable<Post> post)
        {
            var posts = post.Select(Mapper.Map<DTO.Post>).ToList();
            //var history = userRepo.History(user.Id);

            foreach (var p in posts)
            {
                LoadTags(p);
                p.CommentCount = commentRepo.CommentCount(p.Id);
                p.IsCollapsed = p.Description.Length >= 500; //p.Teaser != null;
                p.PublishedIn = postRepo.PublishedIn(p.Id, user.Id).Select(c => Mapper.Map<DTO.Channel>(c)).ToList();
                //p.Read = history.Contains(p.Id);
            }

            return posts;
        }

        private void LoadTags(DTO.Post p)
        {
            p.Tags = postRepo.Tags(p.Id).Select(t => t.Tag).ToList();            
        }

        public DTO.RssFeeder LoadRssFeeder(RssFeeder rssFeeder)
        {
            var result = Mapper.Map<DTO.RssFeeder>(rssFeeder);
            result.Tags = channelRepo.GetRssFeederTags(rssFeeder.Id).ToList();
            return result;
        }

        public DTO.Post LoadPost(User user, Post post)
        {
            var result = Mapper.Map<DTO.Post>(post);
            LoadTags(result);
            //result.Read = userRepo.IsRead(user.Id, post.Id);
            //result.Liked = channelRepo.PartOf(user.LikedChannel, result.Id);
            //result.Starred = channelRepo.PartOf(user.StarredChannel, result.Id);
            //result.Saved = channelRepo.PartOf(user.SavedChannel, result.Id);
            result.Comments = commentRepo.CommentsForPost(post.Id).Select(c => Mapper.Map<DTO.Comment>(c)).ToList();
            result.CommentCount = result.Comments.Count;
            result.PublishedIn = postRepo.PublishedIn(post.Id, user.Id).Select(c => Mapper.Map<DTO.Channel>(c)).ToList();
            return result;            
        }

        public IEnumerable<DTO.Channel> LoadChannelList(User user, IEnumerable<Channel> channels)
        {
            var result = channels.Select(c => Mapper.Map<DTO.Channel>(c)).ToList();

            foreach (var channel in result)
            {
                channel.UnreadCount = channelRepo.UnreadCount(user.Id, channel.Id);
                channel.Subscriptions = channelRepo.GetSubscriptions(channel.Id).Select(Mapper.Map<DTO.Channel>).ToList();
            }

             return result;
        }

        public IEnumerable<DTO.Post> LoadChannelPosts(User user, Channel channel, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy)
        {
            var posts = postRepo.PostsFromChannel(user.Id, onlyUnread, channel.Id, timestamp, fromIndex, orderBy).ToList();
            var commentCounts = commentRepo.CommentCounts(channel.Id);
            var xx = posts.Select(p => Mapper.Map<DTO.Post>(p)).ToList();

            foreach (var p in xx)
            {
                LoadTags(p);

                var commentCount = commentCounts.SingleOrDefault(cc => cc.Id == p.Id);
                p.CommentCount = commentCount != null ? commentCount.Count : 0;
                p.IsCollapsed = true;
                p.PublishedIn = postRepo.PublishedIn(p.Id, user.Id).Select(c => Mapper.Map<DTO.Channel>(c)).ToList();
            }

            return xx;
        }

        public DTO.Channel LoadChannel(User user, Channel channel)
        {
            var result = Mapper.Map<DTO.Channel>(channel);

            //var posts = postRepo.PostsFromChannel(user.Id, onlyUnread, channel.Id, timestamp, fromIndex);
            //var commentCounts = commentRepo.CommentCounts(channel.Id);

            //result.Posts = posts.Select(p => Mapper.Map<DTO.Post>(p)).OrderByDescending(p => p.Created).ToList();

            //foreach (var p in result.Posts)
            //{
            //    LoadTags(p);

            //    var commentCount = commentCounts.SingleOrDefault(cc => cc.Id == p.Id);
            //    p.CommentCount = commentCount != null ? commentCount.Count : 0;
            //    p.IsCollapsed = p.Teaser != null;
            //}

            result.UnreadCount = channelRepo.UnreadCount(user.Id, result.Id);
            result.RssFeeders = channelRepo.GetRssFeedersForChannel(channel.Id).Select(Mapper.Map<DTO.RssFeeder>).ToList();
            result.TagCloud = channelRepo.GetTagCloud(channel.Id).Select(Mapper.Map<DTO.WeightedTag>).ToList();
            result.Subscriptions = channelRepo.GetSubscriptions(channel.Id).Select(Mapper.Map<DTO.Channel>).ToList();

            return result;
        }
    }
}
