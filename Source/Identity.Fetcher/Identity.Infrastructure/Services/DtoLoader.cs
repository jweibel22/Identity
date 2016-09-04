using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Identity.Domain;
using Identity.Infrastructure.DTO;
using Identity.Infrastructure.Repositories;
using Channel = Identity.Domain.Channel;
using Post = Identity.Domain.Post;
using User = Identity.Domain.User;

namespace Identity.Infrastructure.Services
{
    public interface ILoadDtos
    {
        DTO.Post LoadPost(User user, Post post);

        IEnumerable<DTO.Post> LoadPosts(User user, IEnumerable<Post> post);        

        DTO.Channel LoadChannel(User user, Channel channel);

        IEnumerable<DTO.Channel> LoadChannelList(User user, IEnumerable<Channel> channels);

        DTO.RssFeeder LoadRssFeeder(Feed feed);
    }


    public class DtoLoader : ILoadDtos
    {
        private readonly PostRepository postRepo;
        private readonly CommentRepostitory commentRepo;
        private readonly UserRepository userRepo;
        private readonly ChannelRepository channelRepo;
        private readonly IEnumerable<InlineArticleSelector> inlineArticleSelectors;

        public DtoLoader(PostRepository postRepo, CommentRepostitory commentRepo, UserRepository userRepo, ChannelRepository channelRepo, InlineArticleSelectorRepository inlineArticleSelectorRepo)
        {
            this.postRepo = postRepo;
            this.commentRepo = commentRepo;
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
            this.inlineArticleSelectors = inlineArticleSelectorRepo.GetAll();
        }

        public IEnumerable<DTO.Post> LoadPosts(User user, IEnumerable<Post> post)
        {
            using (var pc = new PerfCounter("LoadPosts"))
            {
                return post.Select(p => LoadPost(user, p)).ToList();
            }
        }

        public DTO.Post LoadPost(User user, Post post)
        {
            var p = Mapper.Map<DTO.Post>(post);
            LoadTags(p);
            p.CommentCount = commentRepo.CommentCount(p.Id);
            p.IsCollapsed = p.Description.Length >= 500; //p.Teaser != null;
            p.PublishedIn = postRepo.PublishedIn(new[] { p.Id }, user.Id)
                        .OrderByDescending(pi => pi.Count)
                        .Take(5)
                        .Select(pi => new ChannelReference
                        {
                            Id = pi.ChannelId,
                            Name = pi.ChannelName
                        })
                        .ToList();
            p.CanBeInlined = inlineArticleSelectors.Any(s => p.Uri.Contains(s.UrlPattern));
            return p;
        }

        private void LoadTags(DTO.Post p)
        {
            p.Tags = postRepo.Tags(p.Id).Select(t => t.Tag).ToList();
        }

        public DTO.RssFeeder LoadRssFeeder(Feed feed)
        {
            var result = Mapper.Map<DTO.RssFeeder>(feed);
            result.Tags = channelRepo.GetFeedTags(feed.Id).ToList();
            return result;
        }

        public IEnumerable<DTO.Channel> LoadChannelList(User user, IEnumerable<Channel> channels)
        {
            return channels.Select(c => LoadChannel(user, c)).ToList();
        }

        public DTO.Channel LoadChannel(User user, Channel channel)
        {
            var result = Mapper.Map<DTO.Channel>(channel);

            //result.UnreadCount = channelRepo.UnreadCount(user.Id, result.Id);
            result.RssFeeders = channelRepo.GetFeedsForChannel(channel.Id).Select(Mapper.Map<DTO.RssFeeder>).ToList();
            result.TagCloud = channelRepo.GetTagCloud(channel.Id).Select(Mapper.Map<DTO.WeightedTag>).ToList();
            result.Subscriptions = channelRepo.GetSubscriptions(channel.Id).Select(Mapper.Map<DTO.Channel>).ToList();
            result.DisplaySettings = Mapper.Map <DTO.ChannelDisplaySettings>(channelRepo.GetChannelDisplaySettings(user.Id, channel.Id));

            return result;
        }

        //public IEnumerable<DTO.Post> LoadChannelPosts(User user, Channel channel, bool onlyUnread, DateTimeOffset timestamp, int fromIndex, string orderBy)
        //{
        //    var posts = postRepo.PostsFromChannel(user.Id, onlyUnread, channel.Id, timestamp, fromIndex, orderBy).ToList();
        //    //var commentCounts = commentRepo.CommentCounts(channel.Id);
        //    var xx = posts.Select(p => Mapper.Map<DTO.Post>(p)).ToList();

        //    foreach (var p in xx)
        //    {
        //        //LoadTags(p);
        //        //var commentCount = commentCounts.SingleOrDefault(cc => cc.Id == p.Id);
        //        //p.CommentCount = commentCount != null ? commentCount.Count : 0;
        //        p.IsCollapsed = true;
        //        //p.PublishedIn = postRepo.PublishedIn(p.Id, user.Id).Select(c => Mapper.Map<DTO.Channel>(c)).ToList();
        //    }

        //    return xx;
        //}
    }
}
