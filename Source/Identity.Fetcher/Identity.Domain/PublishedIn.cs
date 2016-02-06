using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class PublishedIn
    {
        public long ChannelId { get; set; }

        public string ChannelName { get; set; }

        public long PostId { get; set; }

        public int Count { get; set; }
    }
}
