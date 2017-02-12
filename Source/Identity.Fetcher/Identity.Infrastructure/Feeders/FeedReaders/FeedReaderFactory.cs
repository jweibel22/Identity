using System;
using Identity.Domain;

namespace Identity.Infrastructure.Feeders.FeedReaders
{
    class FeedReaderFactory
    {
        private readonly TwitterReader twitter;
        private readonly RssReader rss;

        public FeedReaderFactory()
        {
            twitter = new TwitterReader();
            rss = new RssReader();
        }

        public IFeedReader GetReader(Feed feeder)
        {
            switch (feeder.Type)
            {
                case FeedType.Rss:
                    return rss;
                case FeedType.Twitter:
                    return twitter;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}