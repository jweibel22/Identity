using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class Feed
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? LastFetch { get; set; }

        public FeedType Type { get; set; }
    }

    public enum FeedType
    {
        Rss, Twitter
    }

    public class WebScraper
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? LastFetch { get; set; }

        public ScraperAlgorithm Algorithm { get; set; }
    }

    public enum ScraperAlgorithm
    {
        AL
    }
}
