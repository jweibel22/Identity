namespace Identity.Domain.Events
{
    public interface IChannelLinkEvent
    {
        void Apply(ChannelLinkGraph graph);
    }
}