using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RssFeeder.Rss
{
    public class RssReader
    {
        private readonly string url;

        public RssReader(string url)
        {
            this.url = url;
        }

        public SyndicationFeed ReadRss()
        {
            using (var stringReader = new StringReader(ReadAll(url)))
            {
                using (var xmlReader = XmlReader.Create(stringReader))
                {
                    return SyndicationFeed.Load(xmlReader);
                }
            }
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

            //using (var reader = XmlReader.Create(url))
            //{

            //}
        }

    }
}
