using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Identity.Domain;
using Identity.Service.Extensions;
using Identity.Service.Jobs;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Identity.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://flapper-news:oijuud22@dogen.mongohq.com:10014/test2");
            var db = client.GetDatabase("test2");

            //var task = new Task(() => RssFeedRefresher.RefreshFeeds(db));
            var recommender = new Recommender(db);            
            var task = new Task(() => recommender.Recommend());
            //var task = new Task(() => XX(db));
            task.Start();
            task.Wait();
            Console.WriteLine("done");
            Console.ReadLine();
        }


        private static async void XX(IMongoDatabase db)
        {

            var activities = db.GetCollection<Activity>("activities");
            var users = db.GetCollection<User>("users");
            var posts = db.GetCollection<Post>("posts");
            var channels = db.GetCollection<Channel>("channels");

            var allPosts = posts.Find(p => true).ToListAsync().Result;
            var userChannels = channels.Find(c => c.name.Contains("IAm")).ToListAsync().Result;

            foreach (var user in users.Find(c => c.displayName.Contains("IAm")).ToListAsync().Result)
            {
                var channel = userChannels.Single(c => c.id == user.DefaultChannel);
                Console.WriteLine(user.displayName);

                foreach (var postId in channel.posts)
                {
                    var post = allPosts.Single(p => p.id == postId);
                    Console.WriteLine(post.title);
                }

                Console.WriteLine();
            }

            //var myChannel = new Channel { name = "jimmy", posts = new List<ObjectId>(), IsPrivate = false };
            //await channels.InsertOneAsync(myChannel);

            

            //jimmy.Owns = new[] {myChannel.id};
            //jimmy.SubscribesTo = new[] {jp.id};
            //await users.ReplaceOneAsync(c => c.id == jimmy.id, jimmy);

            //Console.WriteLine(jimmy.displayName);

            //foreach (var p in posts.Find(x => true).ToListAsync().Result)
            {
                //Console.WriteLine(p.title);
                //Console.WriteLine(p.id);
                //Console.WriteLine(p.url);
                //Console.WriteLine();

                //p.uri = p.url;
                //p.type = "link";
                //await posts.ReplaceOneAsync(c => c.id == p.id, p);

                //myChannel.AddPost(p.id);    
            }
            //await channels.ReplaceOneAsync(c => c.id == myChannel.id, myChannel);
            //await channels.ReplaceOneAsync(c => c.id == jp.id, jp);            

            //var jpPosts = await posts.FindAsync(p => myChannel.posts.Contains(p.id)).Result.ToListAsync();
            //myActivities.posts = jpPosts.Select(p => new PostEvent { post = p.id, timestamp = p.created }).ToArray();
            //await activities.ReplaceOneAsync(c => c.id == myActivities.id, myActivities);




            
        }

    }
}
