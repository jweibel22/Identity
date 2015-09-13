using System;

namespace Identity.Domain
{
    public class ChannelItem
    {
        public long ChannelId { get; set; }

        public long PostId { get; set; }

        public long UserId { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}