using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.DTO
{
    public class HomeScreen
    {
        public IList<WeightedTag> TagCloud { get; set; }

        public IList<Post> Posts { get; set; }

        public IList<Channel> Channels { get; set; }
    }
}
