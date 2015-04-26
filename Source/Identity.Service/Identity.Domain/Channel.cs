using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Identity.Domain
{
    public class Channel
    {
        public ObjectId id { get; set; }

        public int __v { get; set; }

        public string name { get; set; }

        public IList<ObjectId> posts { get; set; }

        public bool IsPrivate { get; set; }

        public void AddPost(ObjectId post)
        {
            posts.Add(post);
        }
    }

}
