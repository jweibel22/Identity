using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class Channel
    {
        public Channel()
        {
            Subscriptions = new List<DTO.Channel>();
            TagCloud = new List<WeightedTag>();
            ShowUnreadCounter = true;
        }

        public long Id { get; set; }
        
        public string Name { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsLocked { get; set; }

        public int UnreadCount { get; set; }

        public bool ShowUnreadCounter { get; set; }

        public IEnumerable<Channel> Subscriptions { get; set; }

        public IList<WeightedTag> TagCloud { get; set; }

        public ChannelDisplaySettings DisplaySettings { get; set; }

        public ChannelStatistics Statistics { get; set; }
    }

    public class WeightedTag
    {
        public string text { get; set; }

        public decimal weight { get; set; }
    }
}