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
        IEnumerable<DTO.Post> LoadPosts(IEnumerable<Post> post);
        DTO.Post LoadPost(Post post);
        DTO.Channel LoadChannel(Channel channel, bool onlyUnread);
        IEnumerable<DTO.Channel> LoadChannelList(IEnumerable<Channel> channels);
        DTO.RssFeeder LoadRssFeeder(RssFeeder rssFeeder);
    }

    public class DtoLoader : ILoadDtos
    {
        private readonly PostRepository postRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly UserRepository userRepo;
        private readonly ChannelRepository channelRepo;
        private readonly User user;

        public DtoLoader(PostRepository postRepo, CommentRepostitory commentRepo, User user, UserRepository userRepo, ChannelRepository channelRepo)
        {
            this.postRepo = postRepo;
            this.commentRepo = commentRepo;
            this.user = user;
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
        }

        public IEnumerable<DTO.Post> LoadPosts(IEnumerable<Post> post)
        {
            var posts = post.Select(Mapper.Map<DTO.Post>).ToList();
            //var history = userRepo.History(user.Id);

            foreach (var p in posts)
            {
                LoadTags(p);
                p.CommentCount = commentRepo.CommentCount(p.Id);
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

        public DTO.Post LoadPost(Post post)
        {
            var result = Mapper.Map<DTO.Post>(post);
            LoadTags(result);
            result.Read = userRepo.IsRead(user.Id, post.Id);
            result.Liked = channelRepo.PartOf(user.LikedChannel, result.Id);
            result.Starred = channelRepo.PartOf(user.StarredChannel, result.Id);
            result.Saved = channelRepo.PartOf(user.SavedChannel, result.Id);
            result.Comments = commentRepo.CommentsForPost(post.Id).Select(c => Mapper.Map<DTO.Comment>(c)).ToList();
            result.CommentCount = result.Comments.Count;
            return result;            
        }

        public IEnumerable<DTO.Channel> LoadChannelList(IEnumerable<Channel> channels)
        {
            var result = channels.Select(c => Mapper.Map<DTO.Channel>(c)).ToList();

            foreach (var channel in result)
            {
                channel.UnreadCount = channelRepo.UnreadCount(user.Id, channel.Id);
            }

            return result;
        }

        public DTO.Channel LoadChannel(Channel channel, bool onlyUnread)
        {
            var result = Mapper.Map<DTO.Channel>(channel);

            var history = channelRepo.History(user.Id, channel.Id);
            var liked = channelRepo.Intersection(channel.Id, user.LikedChannel);
            var starred = channelRepo.Intersection(channel.Id, user.StarredChannel);
            var saved = channelRepo.Intersection(channel.Id, user.SavedChannel);
            var posts = onlyUnread ? postRepo.UnreadPostsFromChannel(user.Id, onlyUnread, channel.Id) : postRepo.PostsFromChannel(channel.Id);
            var commentCounts = commentRepo.CommentCounts(channel.Id);

            result.Posts = posts.Select(p => Mapper.Map<DTO.Post>(p)).OrderByDescending(p => p.Created).ToList();

            foreach (var p in result.Posts)
            {
                LoadTags(p);
                p.Read = history.Contains(p.Id);
                p.Saved = saved.Contains(p.Id);
                p.Starred = starred.Contains(p.Id);
                p.Liked = liked.Contains(p.Id);

                var commentCount = commentCounts.SingleOrDefault(cc => cc.Id == p.Id);
                p.CommentCount = commentCount != null ? commentCount.Count : 0;
            }

            result.UnreadCount = channelRepo.UnreadCount(user.Id, result.Id);
            result.RssFeeders = channelRepo.GetRssFeedersForChannel(channel.Id).Select(Mapper.Map<DTO.RssFeeder>).ToList();

            return result;
        }
    }
}
