using System.Collections.Generic;

namespace Identity.Infrastructure.DTO
{
    public class Channel
    {
        public long Id { get; set; }
        
        public string Name { get; set; }

        public IList<Post> Posts { get; set; }

        public bool IsPrivate { get; set; }

        public int UnreadCount { get; set; }

        public IList<RssFeeder> RssFeeders { get; set; }
    }
}