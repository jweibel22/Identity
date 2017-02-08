namespace Identity.Domain.Events
{
    public class UserDeleted : IChannelLinkEvent
    {
        public long UserId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            var userNode = graph.GetUserNode(UserId);
            graph.RemoveNode(userNode);

        }
    }
}