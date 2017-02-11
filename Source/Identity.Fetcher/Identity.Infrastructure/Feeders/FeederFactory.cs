using System;
using Identity.Domain;
using Identity.Infrastructure.Feeders.FeedReaders;

namespace Identity.Infrastructure.Feeders
{
    class FeederFactory
    {
        private readonly TwitterReader twitter;
        private readonly RssReader rss;

        public FeederFactory()
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