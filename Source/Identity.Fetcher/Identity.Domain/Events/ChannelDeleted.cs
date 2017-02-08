namespace Identity.Domain.Events
{
    public class ChannelDeleted : IChannelLinkEvent
    {
        public long ChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var node = graph.GetChannelNode(ChannelId);
            graph.RemoveNode(node);
        }
    }
}