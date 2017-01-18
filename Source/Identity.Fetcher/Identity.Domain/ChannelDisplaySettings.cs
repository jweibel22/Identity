using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class ChannelDisplaySettings
    {
        public bool ShowOnlyUnread { get; set; }

        public string OrderBy { get; set; }

        public string ListType { get; set; }

        public bool DraggingEnabled { get; set; }

        public static ChannelDisplaySettings New()
        {
            return new ChannelDisplaySettings
            {
                ListType = "Full",
                OrderBy = "Added",
                ShowOnlyUnread = true,
                DraggingEnabled = false
            };
        }
    }
}
