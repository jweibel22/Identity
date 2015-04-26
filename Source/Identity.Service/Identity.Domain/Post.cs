using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Identity.Domain
{
    [BsonIgnoreExtraElements]
    public class Post
    {
        public ObjectId id { get; set; }

        public DateTime created { get; set; }

        public string title { get; set; }
        public string description { get; set; }

        public string uri { get; set; }

        public object author { get; set; }

        public ObjectId[] comments { get; set; }

        public string[] tags { get; set; }

        public int upvotes { get; set; }

        public string type { get; set; }

        public int __v { get; set; }

        public void Tag(IEnumerable<string> tags)
        {
            this.tags = this.tags.Union(tags).ToArray();
        }

        public void Tag(string tag)
        {
            tags = tags.Union(new[]{tag}).ToArray();
        }
    }
}
