using System;
using System.Collections.Generic;
using Identity.Domain.Events;
using Identity.OAuth;
using Identity.OAuth.EventHandlers;

namespace Identity.Rest
{
    public class ChannelLinkEventBatch
    {
        private readonly ChannelLinkGraphCache graphCache;
        private readonly Bus bus;

        public ChannelLinkEventBatch(ChannelLinkGraphCache graphCache, Bus bus)
        {
            this.graphCache = graphCache;
            this.bus = bus;
            Events = new List<IChannelLinkEvent>();
        }

        private IList<IChannelLinkEvent> Events { get; set; }

        public void Add(IChannelLinkEvent e)
        {
            Events.Add(e);
        }

        public void Commit()
        {
            var dirtyUserChannels = graphCache.Graph.ApplyChanges(Events);

            foreach (var edge in dirtyUserChannels.Channels)
            {
                bus.Publish("refresh-unread-counts", String.Format("{0}-{1}", edge.From.Id, edge.To.Id));
            }
        }
    }
}