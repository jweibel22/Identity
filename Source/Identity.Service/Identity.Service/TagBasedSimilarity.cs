using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Identity.Service
{
    class TagBasedSimilarity
    {
        private BiDirectionalMap<string,int> tagMap = new BiDirectionalMap<string, int>();
        private IDictionary<string, int> tagCount = new Dictionary<string, int>();
        private IDictionary<ObjectId, SimilarityVector> itemTagVector = new Dictionary<ObjectId, SimilarityVector>();

        private List<Post> allPosts;
        private IMongoCollection<Post> postCollection;
        private IMongoCollection<Channel> channelCollection;

        public TagBasedSimilarity(IMongoDatabase db)
        {
            postCollection = db.GetCollection<Post>("posts");
            channelCollection = db.GetCollection<Channel>("channels");
            allPosts = postCollection.Find(p => true).ToListAsync().Result;
            Setup();
        }

        private void Setup()
        {
            int id = 1;
            foreach (var tag in allPosts.Where(post => post.tags != null).SelectMany(post => post.tags).Distinct())
            {
                tagMap.Add(tag,id++);
                tagCount.Add(tag,0);
            }

            foreach (var post in allPosts.Where(post => post.tags != null))
            {             
                itemTagVector.Add(post.id, new SimilarityVector(post.tags.Select(tag => tagMap.Forward[tag])));

                foreach (var tag in post.tags)
                {
                    tagCount[tag] = tagCount[tag] + 1;
                }
            }
        }

        public IList<Recommendation> Recommend(User user)
        {
            var channel = channelCollection.Find(c => c.id == user.DefaultChannel).SingleAsync().Result;

            var userTagVector = new SimilarityVector();

            foreach (var p in channel.posts.Select(x => allPosts.Single(y => y.id == x)))
            {
                userTagVector.Add(p.tags.Select(tag => tagMap.Forward[tag]));
            }

            foreach (var itemId in userTagVector.ItemIds.ToList())
            {
                double totalTagFreq = (double)tagCount[tagMap.Reverse[itemId]] / allPosts.Count;
                double userTagFreq = userTagVector.For(itemId) / channel.posts.Count;
                
                userTagVector.SetFor(itemId, userTagFreq/Math.Max(0.001,totalTagFreq));
            }

            //Console.WriteLine(user.displayName);

            //var sb = new StringBuilder();
            //foreach (var itemId in userTagVector.ItemIds)
            //{
            //    sb.Append(tagMap.Reverse[itemId] + ": " + userTagVector.For(itemId) + ", ");
            //}

            //Console.WriteLine(sb.ToString());

            return allPosts
                .Where(post => post.tags != null && !channel.posts.Contains(post.id))
                .Select(post => new Recommendation
                {
                    ItemId = post.id,
                    Relevance = CosineSimilarity(userTagVector, itemTagVector[post.id])
                })
                .OrderByDescending(r => r.Relevance)
                .ToList();
        }

        private double CosineSimilarity(SimilarityVector v1, SimilarityVector v2)
        {
            double m = v1.ItemIds.Union(v2.ItemIds).Aggregate(0.0, (current, i) => current + v1.For(i)*v2.For(i));
            double a = Math.Sqrt(v1.ItemIds.Aggregate(0.0, (current, i) => current + v1.For(i)*v1.For(i)));
            double b = Math.Sqrt(v2.ItemIds.Aggregate(0.0, (current, i) => current + v2.For(i) * v2.For(i)));

            return m / (a*b);
        }
    }
}
