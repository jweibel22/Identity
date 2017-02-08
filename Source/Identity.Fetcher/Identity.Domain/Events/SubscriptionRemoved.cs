namespace Identity.Domain.Events
{
    public class SubscriptionRemoved : IChannelLinkEvent
    {
        public long UpstreamChannelId { get; set; }

        public long DownstreamChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var edge = graph.GetChannelEdge(UpstreamChannelId, DownstreamChannelId);
            graph.RemoveEdge(edge);
        }
    }
}