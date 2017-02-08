using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Events
{
    public class PostAdded : IChannelLinkEvent
    {
        public long PostId { get; set; }

        public long ChannelId { get; set; }

        public void Apply(ChannelLinkGraph graph)
        {
            graph.MarkAsDirty(ChannelId);
        }
    }
}


