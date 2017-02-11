using System.Collections.Generic;

namespace Identity.Infrastructure.Feeders.FeedReaders
{
    public interface IFeedReader
    {
        IEnumerable<FeedItem> Fetch(string id);
    }
}