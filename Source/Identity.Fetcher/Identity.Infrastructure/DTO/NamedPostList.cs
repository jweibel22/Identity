using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.DTO
{
    public class NamedPostList
    {
        public IList<Post> Posts { get; set; }

        public string Name { get; set; }
    }
}
