using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class Channel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public DateTimeOffset Created { get; set; }

        public bool IsPublic { get; set; }

        public Channel()
        {
        }

        public Channel(string name)
        {
            Created = DateTimeOffset.Now;
            Name = name;
        }
    }
}
