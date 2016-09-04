using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class RssFeeder
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? LastFetch { get; set; }

        public FeederType Type { get; set; }
    }

    public enum FeederType
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
