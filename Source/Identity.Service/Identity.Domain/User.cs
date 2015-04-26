using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Identity.Domain
{
    public class User
    {
        public ObjectId id { get; set; }

        public int __v { get; set; }

        public Identifier local { get; set; }

        public PocketAuth pocket { get; set; }

        public FacebookAuth facebook { get; set; }

        public string displayName { get; set; }

        public IList<ObjectId> Owns { get; set; }

        public IList<ObjectId> SubscribesTo { get; set; }

        public IList<ObjectId> Feed { get; set; }

        public ObjectId DefaultChannel { get; set; }
    }

    public class Identifier
    {
        public string email { get; set; }

        public string password { get; set; }
    }

    public class PocketAuth
    {
        public string accessToken { get; set; }

        public string authCode { get; set; }

        public string username { get; set; }

        public long latestSync { get; set; }
    }

    public class FacebookAuth
    {
        public string name { get; set; }

        public string facebookid { get; set; }
    }
}
