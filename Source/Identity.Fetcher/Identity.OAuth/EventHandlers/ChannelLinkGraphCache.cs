using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Identity.Domain;
using Identity.Infrastructure.Repositories;

namespace Identity.OAuth.EventHandlers
{    
    public class ChannelLinkGraphCache
    {
        public ChannelLinkGraph Graph { get; private set; }

        public ChannelLinkGraphCache(ChannelLinkGraph graph)
        {
            Graph = graph;
        }
    }
}