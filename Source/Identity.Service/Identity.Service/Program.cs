using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Identity.Domain;
using Identity.Service.Extensions;
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


            var task = new Task(() => RefreshFeeds(db));
            task.Start();
            task.Wait();
            Console.WriteLine("done");
            Console.ReadLine();
        }

        private async static void RefreshFeeds(IMongoDatabase db)
        {
            var rssFeeder = new RssFeeder(db);
            var channels = db.GetCollection<Channel>("channels");

            IDictionary<string, ChannelAndTags> rssFeedToChannel = new Dictionary<string, ChannelAndTags>();

            rssFeedToChannel.Add("http://feeds.feedburner.com/upworthy?format=xml", new ChannelAndTags { Name = "upworthy" });
            rssFeedToChannel.Add("http://motherboard.vice.com/en_us/rss", new ChannelAndTags { Name = "motherboard" });
            rssFeedToChannel.Add("http://jyllands-posten.dk/?service=rssfeed&mode=top", new ChannelAndTags { Name = "jyllands-posten", Tags = new[] { "news" } });
            rssFeedToChannel.Add("http://jyllands-posten.dk/nyviden/?service=rssfeed", new ChannelAndTags { Name = "jyllands-posten", Tags = new[] { "science" } });
            rssFeedToChannel.Add("http://www.quora.com/Computer-Programming/rss", new ChannelAndTags { Name = "quora", Tags = new[] { "programming" } });
            rssFeedToChannel.Add("http://www.quora.com/rss", new ChannelAndTags { Name = "quora" });
            rssFeedToChannel.Add("http://gdata.youtube.com/feeds/base/users/Vsauce/uploads?orderby=updated&client=ytapi-youtube-rss-redirect&v=2&alt=rss", new ChannelAndTags { Name = "vsauce", Tags = new[] { "science" } });
            rssFeedToChannel.Add("http://www.dr.dk/nyheder/service/feeds/allenyheder", new ChannelAndTags { Name = "dr", Tags = new[] { "news" } });
            rssFeedToChannel.Add("http://www.dr.dk/mu/Feed/videnskabens-verden?format=podcast&limit=10", new ChannelAndTags { Name = "videnskabens-verden", Tags = new[] { "science" } });
            rssFeedToChannel.Add("http://videnskab.dk/rss", new ChannelAndTags { Name = "videnskab.dk", Tags = new[] { "science" } });
            rssFeedToChannel.Add("http://www.dr.dk/mu/Feed/harddisken?format=podcast&limit=10", new ChannelAndTags { Name = "harddisken", Tags = new[] { "technology" } });
            rssFeedToChannel.Add("http://blog.codinghorror.com/rss/", new ChannelAndTags { Name = "coding-horror", Tags = new[] { "programming" } });
            rssFeedToChannel.Add("http://www.infoq.com/rss", new ChannelAndTags { Name = "infoq", Tags = new[] { "programming" } });
            rssFeedToChannel.Add("http://syndication.thedailywtf.com/TheDailyWtf", new ChannelAndTags { Name = "the-daily-wtf", Tags = new[] { "programming" } });
            rssFeedToChannel.Add("http://feeds2.feedburner.com/tedtalks_video/", new ChannelAndTags { Name = "ted" });
            rssFeedToChannel.Add("http://feeds.gawker.com/lifehacker/full#_ga=1.201702801.1632047189.1426325163", new ChannelAndTags { Name = "lifehacker" });
            rssFeedToChannel.Add("http://www.huffingtonpost.com/feeds/index.xml", new ChannelAndTags { Name = "the-huffington-post" });
            rssFeedToChannel.Add("http://www.dr.dk/mu/Feed/p1-debat-og-soendagsfrokosten-podcast?format=podcast&limit=10", new ChannelAndTags { Name = "p1-debat", Tags = new[] { "p1" } });
            rssFeedToChannel.Add("http://martinfowler.com/feed.atom", new ChannelAndTags { Name = "martin-fowler", Tags = new[] { "programming" } });
            //rssFeedToChannel.Add("http://heltnormalt.dk/?view=rss", new ChannelAndTags { Name = "helt-normalt", Tags = new[] { "comedy" } });

            foreach (var kv in rssFeedToChannel)
            {
                await rssFeeder.Execute(kv.Key, channels.FindOne(c => c.name == kv.Value.Name), p => p.Tag(kv.Value.Tags.Union(new[]{kv.Value.Name})));    
            }

        }

        private static async Task AddChannel(IMongoDatabase db, string name)
        {
            var channels = db.GetCollection<Channel>("channels");

            await channels.InsertOneAsync(new Channel
            {
                name = name,
                posts = new List<ObjectId>()
            });            
        }

        private static async void XX(IMongoDatabase db)
        {

            var activities = db.GetCollection<Activity>("activities");
            var users = db.GetCollection<User>("users");
            var posts = db.GetCollection<Post>("posts");
            var channels = db.GetCollection<Channel>("channels");

            //var myChannel = new Channel { name = "jimmy", posts = new List<ObjectId>(), IsPrivate = false };
            //await channels.InsertOneAsync(myChannel);

            var jp = channels.Find(c => c.id == new ObjectId("551f970b6a09e019bcc3c4bf")).SingleAsync().Result; 
            var myChannel = channels.Find(c => c.id == new ObjectId("551fac8f6a09e00e904eea9b")).SingleAsync().Result;
            
            //myChannel.posts.Clear();
            //await posts.Find(c => true).ForEachAsync(post => myChannel.posts.Add(post.id));
            

            var jimmy = users.Find(u => u.id == new ObjectId("547067477aca47b819961dca")).SingleAsync().Result;
            var myActivities = activities.Find(a => a.id == new ObjectId("552116fd1d4df99439eb7ec2")).SingleAsync().Result;

            
            

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

    class ChannelAndTags
    {
        public ChannelAndTags()
        {
            Tags = new List<string>();
        }

        public string Name { get; set; }

        public IEnumerable<string> Tags { get; set; }
    }
}
