using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class UnreadCount
    {
        public long ChannelId { get; set; }

        public int Count { get; set; }
    }
}
