using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;

namespace Identity.Infrastructure.Feeders.FeedReaders
{
    public class RssReader : IFeedReader
    {
        public IEnumerable<FeedItem> Fetch(string id)
        {
            using (var stringReader = new StringReader(ReadAll(id)))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    var feed = SyndicationFeed.Load(xmlReader);

                    return feed.Items.Select(i => new FeedItem
                    {
                        Content = GetDescription(i),
                        CreatedAt = GetCreatedTime(i),
                        Title = i.Title.Text.Substring(0, Math.Min(i.Title.Text.Length, 250)),
                        Links = i.Links.Select(l => l.Uri).ToList(),
                        Tags = i.Categories.Select(c => c.Name).ToList()
                    });
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

        private string ReadAll(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 30000;

            using (var response = request.GetResponse())
            using (var reader = XmlReader.Create(response.GetResponseStream()))
            {
                var xml = new XmlDocument();
                xml.Load(reader);

                return xml.InnerXml
                    .Replace("<description />", "<description></description>")
                    .Replace("<link />", "<link></link>");
            }
        }

    }
}
