using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Identity.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Identity.Service
{
    class UserBasedSimilarity
    {
        private BiDirectionalMap<ObjectId, int> channelIdMap = new BiDirectionalMap<ObjectId, int>();
        private BiDirectionalMap<ObjectId, int> postIdMap = new BiDirectionalMap<ObjectId, int>();

        private IMongoCollection<Channel> channelCollection;
        private IMongoCollection<Post> postCollection;
        private IMongoCollection<User> userCollection;


        public UserBasedSimilarity(IMongoDatabase db)
        {
            channelCollection = db.GetCollection<Channel>("channels");
            postCollection = db.GetCollection<Post>("posts");
            userCollection = db.GetCollection<User>("users");
            WriteInputFile();
        }

        private IEnumerable<Recommendation> ParseResult()
        {
            using (var reader = new StreamReader(@"c:\transit\tmp\recommendations.txt"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    yield return new Recommendation
                    {
                        ItemId = postIdMap.Reverse[Int32.Parse(line.Split(';')[0])],
                        Relevance = Double.Parse(line.Split(';')[1], CultureInfo.GetCultureInfo("en-GB"))
                    };
                }
            }
        }

        private void WriteInputFile()
        {
            var userChannels = channelCollection.Find(c => c.name.Contains("IAm")).ToListAsync().Result;

            int id = 1;
            foreach (var c in userChannels)
            {
                channelIdMap.Add(c.id, id++);
            }

            var posts = postCollection.Find(p => true).ToListAsync().Result;

            id = 1;
            foreach (var c in posts)
            {
                postIdMap.Add(c.id, id++);
            }

            var map =
                from channel in userChannels
                from post in channel.posts
                select new KeyValuePair<int, int>(channelIdMap.Forward[channel.id], postIdMap.Forward[post]);

            using (var writer = new StreamWriter(@"c:\transit\tmp\posts.csv"))
            {
                foreach (var kv in map)
                {
                    writer.WriteLine("{0},{1},1", kv.Key, kv.Value);
                }
            }

            //using (var writer = new StreamWriter(@"c:\transit\tmp\postMap.csv"))
            //{
            //    foreach (var c in posts)
            //    {
            //        writer.WriteLine("{0}:{1}", postIdMap.Forward[c.id], c.title);
            //    }
            //}      
        }

        public IList<Recommendation> Recommend(User user)
        {
            var channelId = channelIdMap.Forward[user.DefaultChannel];

            var process = new Process();
            process.StartInfo.FileName = "java.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = @"C:\git\Identity\Source\Recommendations\target\Recommendations-1.0-SNAPSHOT";
            process.StartInfo.Arguments = @"-cp Recommendations-1.0-SNAPSHOT.jar;lib\* Program " + channelId;
            process.EnableRaisingEvents = true;
            process.Start();

            if (process.WaitForExit(30000))
            {
                return ParseResult().ToList();    
            }
            else
            {
                throw new Exception("User based recommendations timed out");
            }
           
        }
    }
}
