﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.DTO
{
    public class ChannelDisplaySettings
    {
        public bool ShowOnlyUnread { get; set; }

        public string OrderBy { get; set; }

        public string ListType { get; set; }

        public bool DraggingEnabled { get; set; }
    }

    public class ChannelStatistics
    {
        public int PostPerDay { get; set; }

        public int Popularity { get; set; }
    }
}
