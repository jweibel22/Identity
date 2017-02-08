namespace Identity.Domain.Events
{
    public class ChannelAdded : IChannelLinkEvent
    {
        public long ChannelId { get; set; }

        public void Apply(ChannelLinkGraph graph)
        {
            var node = new ChannelLinkNode
            {
                Id = ChannelId,
                NodeType = NodeType.Channel
            };

            graph.AddNode(node);
        }
    }
}