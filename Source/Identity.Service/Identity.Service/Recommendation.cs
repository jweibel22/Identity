using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Identity.Service
{
    class Recommendation
    {
        public ObjectId ItemId { get; set; }

        public double Relevance { get; set; }
    }
}
