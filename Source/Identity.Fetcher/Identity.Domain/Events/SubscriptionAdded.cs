namespace Identity.Domain.Events
{
    public class SubscriptionAdded : IChannelLinkEvent
    {
        public long UpstreamChannelId { get; set; }

        public long DownstreamChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var downstreamChannelNode = graph.GetChannelNode(DownstreamChannelId);
            var upstreamChannelNode  = graph.GetChannelNode(UpstreamChannelId);
            graph.AddEdge(new ChannelLinkEdge { From = upstreamChannelNode, To = downstreamChannelNode });
            graph.MarkAsDirty(DownstreamChannelId);
        }
    }
}