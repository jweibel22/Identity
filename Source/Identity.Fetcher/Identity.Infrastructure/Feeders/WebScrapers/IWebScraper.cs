using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Infrastructure.WebScrapers
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
