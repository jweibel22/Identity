namespace Identity.Domain.Events
{
    public class UserLeaves : IChannelLinkEvent
    {
        public long UserId { get; set; }

        public long ChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var edge = graph.GetUserEdge(UserId, ChannelId);
            graph.RemoveEdge(edge);
        }
    }
}