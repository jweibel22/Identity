using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Fetcher.WindowsService
{
    class ChannelAndTags
    {
        public ChannelAndTags()
        {
            Tags = new List<string>();
        }

        public string Name { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
