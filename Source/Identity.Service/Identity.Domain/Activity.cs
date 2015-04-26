using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Identity.Domain
{
    public class Activity
    {
        public ObjectId id { get; set; }

        public int __v { get; set; }

        public IList<PostEvent> posts { get; set; }

        public IList<PostEvent> comments { get; set; }

        public IList<PostEvent> likes { get; set; }

        public ObjectId user { get; set; }
    }

    public class PostEvent
    {
        public ObjectId id { get; set; }

        public ObjectId post { get; set; }

        public DateTime timestamp { get; set; }
    }
}
