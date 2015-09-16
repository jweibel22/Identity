using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class Channel
    {
        public long Id { get; set; }
        
        public string Name { get; set; }

        public IList<Post> Posts { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsLocked { get; set; }

        public int UnreadCount { get; set; }

        public IList<RssFeeder> RssFeeders { get; set; }

        public IEnumerable<Channel> Subscriptions { get; set; }

        public IList<WeightedTag> TagCloud { get; set; }
    }

    public class WeightedTag
    {
        public string text { get; set; }

        public decimal weight { get; set; }
    }
}