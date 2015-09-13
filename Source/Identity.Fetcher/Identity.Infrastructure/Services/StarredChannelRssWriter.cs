using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using Identity.Domain;
using Identity.Infrastructure.Repositories;

namespace Identity.Infrastructure.Services
{
    public class StarredChannelRssWriter
    {
        private readonly PostRepository postRepo;
        private readonly UserRepository userRepo;

        public StarredChannelRssWriter(PostRepository postRepo, UserRepository userRepo)
        {
            this.postRepo = postRepo;
            this.userRepo = userRepo;
        }

        public void Write(User user)
        {            
            var posts = Enumerable.Range(0, 5)
                                  .SelectMany(
                                    i => postRepo.PostsFromChannel(user.Id, false, user.StarredChannel, DateTimeOffset.Now, i*30, "Added"));

            var items = new List<SyndicationItem>();

            foreach (var p in posts)
            {
                var item = new SyndicationItem
                {
                    Title = new TextSyndicationContent(p.Title),
                    Summary = new TextSyndicationContent(p.Description),
                    PublishDate = DateTime.Now
                    
                };
                item.Links.Add(new SyndicationLink(new Uri(p.Uri)));

                items.Add(item);
            }
            var rssFilename = String.Format("{0}_starred_v2.rss",user.Username.ToLowerInvariant());

            var feed = new SyndicationFeed("Jimmys Starred", "All the starred posts from " + user.Username, new Uri(String.Format("https://dl.dropboxusercontent.com/u/41035838/{0}", rssFilename)), items);


            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;
            Stream stream = new MemoryStream();

            using (var writer = XmlWriter.Create(stream, settings))
            {
                feed.SaveAsRss20(writer);    
            }

            stream.Position = 0;
            var xml = new XmlDocument();
            xml.Load(stream);

            foreach (XmlNode n in xml.SelectNodes("rss/channel/item"))
            {
                XmlNode link = null;

                foreach (XmlNode c in n.ChildNodes)
                {
                    if (c.Name == "a10:link")
                    {
                        link = c;
                        break;
                    }
                }

                if (link != null)
                {
                    n.RemoveChild(link);
                    var newLink = xml.CreateElement("link");
                    newLink.InnerText = link.Attributes["href"].Value;
                    n.AppendChild(newLink);
                }
            }

            xml.Save(String.Format(@"C:\Users\jwe\Dropbox\Public\{0}", rssFilename));
            
        }
    
    }
}
