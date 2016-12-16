using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using AutoMapper;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using Channel = Identity.Infrastructure.DTO.Channel;
using Post = Identity.Infrastructure.DTO.Post;
using User = Identity.Infrastructure.DTO.User;

namespace Identity.Rest.Api
{
    [UnitOfWorkCommit]
    public class UserController : ApiController
    {
        private readonly UserRepository userRepo;
        private readonly ChannelRepository channelRepo;

        public UserController(UserRepository userRepo, ChannelRepository channelRepo)
        {
            this.userRepo = userRepo;
            this.channelRepo = channelRepo;
        }

        public User Get(int id)
        {
            var identity = User.Identity as ClaimsIdentity;
            var loggedInUser = userRepo.FindByName(identity.Name);

            var user = userRepo.GetById(id);

            if (user == null)
            {
                return null;
            }

            var u = Map(user, loggedInUser);
            return u;
        }


        private Infrastructure.DTO.Channel MapChannel(OwnChannel root, IList<OwnChannel> ownedByUser, IList<ChannelLink> channelLinks)
        {
            var channel = Mapper.Map<Channel>(root);

            var subs = channelLinks
                            .Where(l => l.DownStreamChannelId == channel.Id && ownedByUser.Any(x => x.Id == l.UpStreamChannelId))
                            .Select(l => l.UpStreamChannelId);
            channel.Subscriptions = ownedByUser.Where(x => subs.Contains(x.Id)).Select(x => MapChannel(x, ownedByUser, channelLinks)).ToList();

            return channel;
        }

        private User Map(Domain.User user, Domain.User loggedInUser)
        {
            var channelLinks = userRepo.ChannelLinks(user.Id).ToList();
            var ownedByUser = userRepo.Owns(user.Id).ToList();
            var ownedAsDto = ownedByUser.Select(c => MapChannel(c, ownedByUser, channelLinks)).ToList();           

            return new User
            {
                Id = user.Id,
                DisplayName = user.Username,
                Feed = new List<Post>(),
                FollowsChannels = channelRepo.GetSubscriptions(user.SubscriptionChannel).Select(c => Mapper.Map<Channel>(c)).ToList(),
                FollowsTags = new List<string>(),
                Owns = ownedAsDto,
                ChannelMenuItems = ownedAsDto.Where(x => !ownedAsDto.Any(y => y.Subscriptions.Any(s => s.Id == x.Id))).ToList(),
                SavedChannel = user.SavedChannel,
                StarredChannel = user.StarredChannel,
                LikedChannel = user.LikedChannel,
                SubscriptionChannel = user.SubscriptionChannel,
                TagCloud = userRepo.GetTagCloud(user.Id, loggedInUser.Id).Select(Mapper.Map<Infrastructure.DTO.WeightedTag>).ToList()
            };
        }

        public User Get()
        {
            var identity = User.Identity as ClaimsIdentity;
            var user = userRepo.TryFindByName(identity.Name);

            if (user == null)
            {
                return null;
            }

            var u = Map(user, user);
            return u;
        }

        [HttpGet]
        public IEnumerable<User> Get(string query)
        {
            var identity = User.Identity as ClaimsIdentity;
            var user = userRepo.FindByName(identity.Name);

            return userRepo.SearchByName(query).Select(x => Map(x, user)).ToList();
        }

        [HttpPost]
        [Route("Api/User/{id}/Block")]
        public void Block(long id, string tag)
        {
            userRepo.BlockTag(id, tag);
        }
    }
}
