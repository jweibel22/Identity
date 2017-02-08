namespace Identity.Domain.Events
{
    public class UserAdded : IChannelLinkEvent
    {
        public long UserId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            graph.AddNode(new ChannelLinkNode { Id = UserId, NodeType = NodeType.User });
        }
    }
}