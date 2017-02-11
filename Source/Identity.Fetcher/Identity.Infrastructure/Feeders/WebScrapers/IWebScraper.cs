using System;
using System.Collections.Generic;

namespace Identity.Infrastructure.Feeders.WebScrapers
{
    public interface IWebScraper
    {
        IEnumerable<WebArticle> Scrape();
    }

    public class WebArticle
    {
        public string Url { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public DateTime Updated { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
