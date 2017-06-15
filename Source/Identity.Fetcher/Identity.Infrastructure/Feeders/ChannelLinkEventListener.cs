using System.Collections.Generic;
using Identity.Domain.Events;

namespace Identity.Infrastructure.Feeders
{
    public class ChannelLinkEventListener : IChannelLinkEventListener
    {
        readonly IList<IChannelLinkEvent> events = new List<IChannelLinkEvent>();

        public IList<IChannelLinkEvent> Events
        {
            get { return events; }
        }

        public void Add(IChannelLinkEvent e)
        {
            events.Add((e));
        }
    }
}