using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;
using Identity.Domain;
using Identity.Infrastructure.Repositories;
using Newtonsoft.Json.Linq;

namespace Identity.OAuth
{
    public interface IArticleContentsFetcher
    {
        string Fetch(string url);
    }

    public class MercuryArticleContentsFetcher : IArticleContentsFetcher
    {
        public string Fetch(string url)
        {
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("x-api-key", "WPDWyRQP3PGbktd9wB4UYGplQEDR778RrySYaAGv");
                var result = webClient.DownloadString(String.Format("https://mercury.postlight.com/parser?url={0}", url));

                dynamic json = JObject.Parse(result);
                return json.content;                
            }
        }
    }

    public class ArticleContentsFetcher : IArticleContentsFetcher
    {
        private readonly IEnumerable<InlineArticleSelector> inlineArticleSelectors;

        public ArticleContentsFetcher(InlineArticleSelectorRepository inlineArticleSelectorRepo)
        {
            this.inlineArticleSelectors = inlineArticleSelectorRepo.GetAll();
        }

        public string Fetch(string url)
        {
            using (var webClient = new WebClient())
            {
                var selector = inlineArticleSelectors.FirstOrDefault(s => url.Contains(s.UrlPattern));

                if (selector == null)
                {
                    return "";
                }

                webClient.Encoding = Encoding.UTF8;
                var result = webClient.DownloadString(url);

                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                var elm = doc.DocumentNode.SelectSingleNode(selector.Selector);

                if (elm == null)
                {
                    return "";
                }

                return elm.InnerHtml.Replace("\r\n", "").Replace("\n", "").Replace("\t", "");
            }

        }
    }
}