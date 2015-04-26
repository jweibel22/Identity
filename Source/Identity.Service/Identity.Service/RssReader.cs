using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Identity.Service
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
            using (var reader = XmlReader.Create(url))
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
