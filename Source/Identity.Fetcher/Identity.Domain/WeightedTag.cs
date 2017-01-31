using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class WeightedTag
    {
        public string Text { get; set; }

        public decimal Weight { get; set; }
    }

    public class ChannelScore
    {
        public long ChannelId { get; set; }

        public string ChannelName { get; set; }

        public double Score { get; set; }
    }
}
