using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Service.Extensions;
using MongoDB.Driver;

namespace Identity.Service.Jobs
{
    class RssFeedRefresher
    {
        public async static void RefreshFeeds(IMongoDatabase db)
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
                await rssFeeder.Execute(kv.Key, channels.FindOne(c => c.name == kv.Value.Name), p => p.Tag(kv.Value.Tags.Union(new[] { kv.Value.Name })));
            }

        }
    }
}
