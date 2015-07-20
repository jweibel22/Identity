using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Identity.Domain;
using Identity.Infrastructure.Repositories;

namespace Identity.Fetcher.WindowsService.Rss
{
    class RssFeedRefresher
    {
        private ChannelRepository channelRepo;
        private PostRepository postRepo;
        private UserRepository userRepo;
        private string rssFeederUsername;

        public RssFeedRefresher(ChannelRepository channelRepo, PostRepository postRepo, UserRepository userRepo)
        {
            this.channelRepo = channelRepo;
            this.postRepo = postRepo;
            this.userRepo = userRepo;
            rssFeederUsername = "rssfeeder";
        }

        public void RefreshFeeds()
        {            
            IDictionary<string, ChannelAndTags> rssFeedToChannel = new Dictionary<string, ChannelAndTags>();

            rssFeedToChannel.Add("http://feeds.feedburner.com/upworthy?format=xml", new ChannelAndTags { Name = "upworthy" });
            rssFeedToChannel.Add("http://motherboard.vice.com/en_us/rss", new ChannelAndTags { Name = "motherboard" });
            rssFeedToChannel.Add("http://jyllands-posten.dk/?service=rssfeed&mode=top", new ChannelAndTags { Name = "jyllands-posten", Tags = new[] { "news" } });
            rssFeedToChannel.Add("http://jyllands-posten.dk/nyviden/?service=rssfeed", new ChannelAndTags { Name = "jyllands-posten", Tags = new[] { "science" } });
            rssFeedToChannel.Add("http://www.quora.com/Computer-Programming/rss", new ChannelAndTags { Name = "quora", Tags = new[] { "programming" } });
            rssFeedToChannel.Add("http://www.quora.com/rss", new ChannelAndTags { Name = "quora" });
            //rssFeedToChannel.Add("http://gdata.youtube.com/feeds/base/users/Vsauce/uploads?orderby=updated&client=ytapi-youtube-rss-redirect&v=2&alt=rss", new ChannelAndTags { Name = "vsauce", Tags = new[] { "science" } });
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
                try
                {
                    Execute(kv.Key, kv.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed synchronize with rss feed " + kv.Key + "\r\n" + ex.Message);
                }                
            }
        }

        private void Execute(string rssFeedUrl, ChannelAndTags channelAndTags)
        {
            var rssFeederUser = userRepo.FindByName(rssFeederUsername);

            var rssReader = new RssReader(rssFeedUrl);

            var feed = rssReader.ReadRss();

            foreach (var item in feed.Items)
            {
                try
                {
                    var channel = channelRepo.FindChannelsByName(channelAndTags.Name).SingleOrDefault();

                    if (channel == null)
                    {
                        channel = new Channel(channelAndTags.Name);

                        channelRepo.AddChannel(channel);
                    }

                    var url = item.Links.First().Uri.ToString();


                    if (!postRepo.PostExists(url))
                    {
                        var post = new Post
                        {
                            Created = GetCreatedTime(item),
                            Description = GetDescription(item),
                            Title = item.Title.Text,
                            Uri = url,
                        };

                        postRepo.AddPost(post);
                        postRepo.TagPost(post.Id, item.Categories.Select(c => c.Name).Union(channelAndTags.Tags).Union(new[]{channelAndTags.Name}));
                        
                        userRepo.Publish(rssFeederUser.Id, channel.Id, post.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to add item " + item.Title.Text + "\r\n" + ex.Message);
                }
            }
        }

        private DateTime GetCreatedTime(SyndicationItem item)
        {
            if (item.PublishDate != DateTimeOffset.MinValue)
            {
                return item.PublishDate.ToUniversalTime().DateTime;
            }
            else if (item.LastUpdatedTime != DateTimeOffset.MinValue)
            {
                return item.LastUpdatedTime.ToUniversalTime().DateTime;
            }
            else
            {
                return DateTime.UtcNow;
            }
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

    }
}
