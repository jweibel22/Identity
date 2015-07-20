using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Identity.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Identity.Service.Jobs
{
    class Recommender
    {
        private readonly IMongoCollection<User> userCollection;
        private readonly IMongoCollection<Post> postCollection;
        private readonly UserBasedSimilarity userBasedSimilarity;
        private readonly TagBasedSimilarity tagBasedSimilarity;

        public Recommender(IMongoDatabase db)
        {
            userCollection = db.GetCollection<User>("users");
            postCollection = db.GetCollection<Post>("posts");
            userBasedSimilarity = new UserBasedSimilarity(db);
            tagBasedSimilarity = new TagBasedSimilarity(db);
        }


        public async void Recommend()
        {
            var users = userCollection.Find(c => c.displayName.Contains("IAm")).ToListAsync().Result;
            var allPosts = postCollection.Find(p => true).ToListAsync().Result;

            foreach (var user in users)
            {
                var tagBasedRecommendations = tagBasedSimilarity.Recommend(user);
                var userBasedRecommendations = userBasedSimilarity.Recommend(user);

                var joined = userBasedRecommendations.Union(tagBasedRecommendations)
                    .GroupBy(r => r.ItemId)
                    .Select(g => new Recommendation
                    {
                        ItemId = g.Key,
                        Relevance = g.Sum(r => r.Relevance)
                    });


                var x = userBasedRecommendations.Join(tagBasedRecommendations, r => r.ItemId, r => r.ItemId,
                    (r1, r2) => new
                    {
                        ItemId = r1.ItemId,
                        Relevance1 = r1.Relevance,
                        Relevance2 = r2.Relevance
                    });

                Console.WriteLine(user.displayName);

                foreach (var xx in x)
                {
                    var post = allPosts.Single(p => p.id == xx.ItemId);
                    Console.WriteLine("{0}: {1} + {2}", post.title, xx.Relevance1, xx.Relevance2);
                }

                //foreach (var r in joined.OrderByDescending(r => r.Relevance))
                //{
                //    var post = allPosts.Single(p => p.id == r.ItemId);
                //    Console.WriteLine("{0}: {1} ({2})", r.Relevance, post.title, ToString(post.tags));
                //}

                //user.Feed = joined.OrderByDescending(r => r.Relevance).Take(15).Select(r => r.ItemId).ToList();
                //await userCollection.ReplaceOneAsync(c => c.id == user.id, user);
            }
        }

        private string ToString(string[] ss)
        {
            var sb = new StringBuilder();
            foreach (var s in ss)
            {
                sb.Append(s + ", ");
            }
            return sb.ToString();
        }
    }
}
