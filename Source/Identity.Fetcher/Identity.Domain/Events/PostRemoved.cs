namespace Identity.Domain.Events
{
    public class PostRemoved : IChannelLinkEvent
    {
        public long PostId { get; set; }

        public long ChannelId { get; set; }
        public void Apply(ChannelLinkGraph graph)
        {
            graph.MarkAsDirty(ChannelId);
        }
    }
}