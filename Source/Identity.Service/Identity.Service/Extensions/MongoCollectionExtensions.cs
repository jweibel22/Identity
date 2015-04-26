using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;

namespace Identity.Service.Extensions
{
    static class MongoCollectionExtensions
    {
        public static T FindOne<T>(this IMongoCollection<T> collection, Expression<Func<T,bool>> predicate)
        {
            return collection.Find(predicate).SingleAsync().Result;
        }
    }
}
