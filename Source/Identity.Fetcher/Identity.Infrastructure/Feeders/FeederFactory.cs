using System;
using Identity.Domain;
using Identity.Infrastructure.Feeders;

namespace Identity.Infrastructure.Rss
{
    class FeederFactory
    {
        private readonly TwitterFeeder twitter;
        private readonly RssReader rss;

        public FeederFactory()
        {
            twitter = new TwitterFeeder();
            rss = new RssReader();
        }

        public IFeederReader GetReader(Feed feeder)
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