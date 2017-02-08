namespace Identity.Domain.Events
{
    public class UserJoins : IChannelLinkEvent
    {
        public long UserId { get; set; }

        public long ChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var userNode = graph.GetUserNode(UserId);
            var channelNode = graph.GetChannelNode(ChannelId);
            graph.AddEdge(new ChannelLinkEdge { From = channelNode, To = userNode });
            graph.MarkAsDirty(ChannelId);
        }
    }
}