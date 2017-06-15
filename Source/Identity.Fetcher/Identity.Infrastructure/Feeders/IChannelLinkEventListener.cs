using Identity.Domain.Events;

namespace Identity.Infrastructure.Feeders
{
    public interface IChannelLinkEventListener
    {
        void Add(IChannelLinkEvent e);
    }
}