using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class User
    {
        public long Id { get; set; }

        public string DisplayName { get; set; }

        public IList<string> FollowsTags { get; set; }

        public IList<Channel> FollowsChannels { get; set; }

        public IList<Channel> Owns { get; set; }

        public long SavedChannel { get; set; }

        public long StarredChannel { get; set; }

        public long LikedChannel { get; set; }

        public IList<Post> Feed { get; set; }

        public IList<WeightedTag> TagCloud { get; set; }
    }
}