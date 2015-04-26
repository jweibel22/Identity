using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Identity.Domain;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Identity.Service
{
    class RssFeeder
    {
        private readonly IMongoCollection<Channel> channels;
        private readonly IMongoCollection<Post> posts;

        public RssFeeder(IMongoDatabase db)
        {
            posts = db.GetCollection<Post>("posts");
            channels = db.GetCollection<Channel>("channels");
        }

        private string ReadContent(SyndicationItem item)
        {            
            var contents = 
                from extension in item.ElementExtensions 
                select extension.GetObject<XElement>() into ele 
                where ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content") 
                select ele.Value;
            
                return contents.FirstOrDefault();
        }

        private string GetDescription(SyndicationItem item)
        {
            var result = ReadContent(item);

            if (result != null)
                return result;

            return item.Summary == null ? "" : item.Summary.Text;
        }

        public async Task Execute(string rssFeedUrl, Channel channel, Action<Post> f = null)
        {                        
            var rssReader = new RssReader(rssFeedUrl);
            
            var feed = rssReader.ReadRss();

            foreach (var item in feed.Items)
            {
                try
                {
                    var url = item.Links.First().Uri.ToString();

                    var post = posts.Find(p => p.uri == url && p.type == "link").SingleOrDefaultAsync().Result;

                    if (post == null)
                    {
                        post = new Post
                        {
                            created = item.PublishDate.ToUniversalTime().DateTime,
                            description = GetDescription(item),
                            title = item.Title.Text,
                            uri = url,
                            type = "link",
                            comments = new ObjectId[0],
                            tags = item.Categories.Select(c => c.Name).ToArray(),
                            upvotes = 0,
                        };

                        if (f != null)
                        {
                            f(post);
                        }

                        await posts.InsertOneAsync(post);
                        channel.AddPost(post.id);
                    }
                    else
                    {
                        //if (f != null)
                        //{
                        //    f(post);
                        //}
                        post.description = GetDescription(item);
                        await posts.ReplaceOneAsync(c => c.id == post.id, post);
                    }
                    //else if (!channel.posts.Any(p => p.Equals(post.id)))
                    //{
                    //    channel.AddPost(post.id);
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to add item " + item.Title.Text + "\r\n" + ex.Message);
                }
            }

            await channels.ReplaceOneAsync(c => c.id == channel.id, channel);            
        }                    
    }
}
