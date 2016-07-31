using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class Channel
    {
        public Channel()
        {
            Subscriptions = new List<DTO.Channel>();
            TagCloud = new List<WeightedTag>();
            RssFeeders = new List<RssFeeder>();
        }

        public long Id { get; set; }
        
        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsLocked { get; set; }

        public int UnreadCount { get; set; }

        public bool ShowOnlyUnread { get; set; }

        public string OrderBy { get; set; }

        public string ListType { get; set; }

        public IList<RssFeeder> RssFeeders { get; set; }

        public IEnumerable<Channel> Subscriptions { get; set; }

        public IList<WeightedTag> TagCloud { get; set; }

        public ChannelDisplaySettings DisplaySettings { get; set; }
    }

    public class WeightedTag
    {
        public string text { get; set; }

        public decimal weight { get; set; }
    }
}