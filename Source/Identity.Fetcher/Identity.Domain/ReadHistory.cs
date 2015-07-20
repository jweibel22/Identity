using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class ReadHistory
    {
        public long UserId { get; set; }

        public long PostId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
