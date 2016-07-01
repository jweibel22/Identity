using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Identity.Infrastructure.DTO;

namespace Identity.OAuth
{
    public class SyndicationFeedFormatter : MediaTypeFormatter
    {
        private readonly string atom = "application/atom+xml";
        private readonly string rss = "application/rss+xml";

        public SyndicationFeedFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(atom));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(rss));
        }

        //public SyndicationFeedFormatter(string format)
        //{
        //    //this.AddUriPathExtensionMapping("rss", new MediaTypeHeaderValue(format));
        //    this.AddQueryStringMapping("formatter", "rss", new MediaTypeHeaderValue(format));
        //} 

        Func<Type, bool> SupportedType = (type) =>
        {
            if (type == typeof(NamedPostList))
                return true;
            else
                return false;
        };

        public override bool CanReadType(Type type)
        {
            return SupportedType(type);
        }

        public override bool CanWriteType(Type type)
        {
            return SupportedType(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                if (type == typeof(NamedPostList))
                    BuildSyndicationFeed(value, writeStream, content.Headers.ContentType.MediaType);
            });
        }

        private void BuildSyndicationFeed(object models, Stream stream, string contenttype)
        {
            var channel = ((NamedPostList)models);

            var items = new List<SyndicationItem>();
            var feed = new SyndicationFeed()
            {
                Title = new TextSyndicationContent(channel.Name)
            };          
                
            var enumerator = channel.Posts.GetEnumerator();
            while (enumerator.MoveNext())
            {
                items.Add(BuildSyndicationItem(enumerator.Current));
            }

            feed.Items = items;

            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                if (string.Equals(contenttype, atom))
                {
                    var atomformatter = new Atom10FeedFormatter(feed);
                    atomformatter.WriteTo(writer);
                }
                else
                {
                    var rssformatter = new Rss20FeedFormatter(feed);
                    rssformatter.WriteTo(writer);
                }
            }
        }

        private SyndicationItem BuildSyndicationItem(Post p)
        {
            var item = new SyndicationItem
            {
                Title = new TextSyndicationContent(p.Title),
                Summary = new TextSyndicationContent(p.Description),
                PublishDate = p.Added,
                LastUpdatedTime = p.Added
            };
            item.Links.Add(new SyndicationLink(new Uri(p.Uri)));

            return item;
        }
    }
}