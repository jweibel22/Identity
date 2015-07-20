using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class ChannelOwner
    {
        public long ChannelId { get; set; }

        public long UserId { get; set; }
    }
}
