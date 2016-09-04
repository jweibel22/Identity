using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;

namespace Identity.Infrastructure.DTO
{
    public class RssFeeder
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public IList<string> Tags { get; set; }

        public FeedType Type { get; set; }
    }
}
