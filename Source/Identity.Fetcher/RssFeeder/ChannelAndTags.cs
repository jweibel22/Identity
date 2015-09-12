using System.Collections.Generic;

namespace RssFeeder
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
