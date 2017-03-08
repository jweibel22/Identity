using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Infrastructure.Repositories;

namespace Identity.Infrastructure.Services
{
    public class UserFactory
    {
        private readonly ChannelRepository channelRepository;
        private readonly UserRepository userRepository;

        public UserFactory(ChannelRepository channelRepository, UserRepository userRepository)
        {
            this.channelRepository = channelRepository;
            this.userRepository = userRepository;
        }

        public User CreateNewUser(string username, bool isAnon)
        {
            var channelUsername = isAnon ? "Anon" : username;
            var starredChannel = Channel.New(String.Format("{0}'s starred", channelUsername));
            var savedChannel = Channel.New(String.Format("{0}'s saved", channelUsername));
            var likedChannel = Channel.New(String.Format("{0}'s liked", channelUsername));
            var inbox = Channel.New(String.Format("{0}'s inbox", channelUsername));
            var subscriptionChannel = Channel.New(String.Format("{0}'s subscriptions", channelUsername));

            channelRepository.AddChannel(starredChannel);
            channelRepository.AddChannel(savedChannel);
            channelRepository.AddChannel(likedChannel);
            channelRepository.AddChannel(inbox);
            channelRepository.AddChannel(subscriptionChannel);


            var user = new User
            {
                IdentityId = Guid.NewGuid().ToString(),
                Username = username,
                StarredChannel = starredChannel.Id,
                LikedChannel = likedChannel.Id,
                SavedChannel = savedChannel.Id,
                Inbox = inbox.Id,
                SubscriptionChannel = subscriptionChannel.Id
            };

            if (isAnon)
            {
                userRepository.AddAnonUser(user);
            }
            else
            {
                userRepository.AddUser(user);
            }

            userRepository.Owns(user.Id, starredChannel.Id, true);
            userRepository.Owns(user.Id, savedChannel.Id, true);
            userRepository.Owns(user.Id, likedChannel.Id, true);
            userRepository.Owns(user.Id, inbox.Id, true);
            userRepository.Owns(user.Id, subscriptionChannel.Id, true);

            return user;
        }
    }
}
