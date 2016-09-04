using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Identity.Domain;

namespace Identity.Infrastructure.WebScrapers
{
    public class AlScraper : IWebScraper
    {
        private readonly string url;

        public AlScraper(string url)
        {
            this.url = url;
        }

        //private Gender ParseGender(string s)
        //{
        //    if (s.Contains("label-female"))
        //        return Gender.F;
        //    else if (s.Contains("label-male"))
        //        return Gender.M;
        //    else
        //        return Gender.Unknown;
        //}

        enum Region
        {
            Unknown = 0, DK = 1, KBH = 2, NSJ = 3, MSJ = 4, SSJ = 5, F = 6, NJ = 7, MJ = 8, SJ = 9, J = 10, BH = 11
        }


        private Region ParseRegion(string s)
        {
            Match m2 = Regex.Match(s, @"/search\?region=(.*?)$", RegexOptions.Singleline);
            if (m2.Success)
            {
                return (Region)Int32.Parse(m2.Groups[1].Value);
            }
            else
            {
                return Region.Unknown;
            }
        }

        private string ParsePoster(string s)
        {
            Match m2 = Regex.Match(s, @"/profile/(.*?)$", RegexOptions.Singleline);
            if (m2.Success)
            {
                return m2.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private DateTime ParseUpdated(string s)
        {
            Match m2 = Regex.Match(s, @"I dag, kl. (.*?):(.*?)$", RegexOptions.Singleline);
            if (m2.Success)
            {
                var hour = Int32.Parse(m2.Groups[1].Value);
                var minute = Int32.Parse(m2.Groups[2].Value);
                return DateTime.Today.AddHours(hour).AddMinutes(minute);
            }
            else
            {
                return DateTime.ParseExact(s, "dd/MM yyyy HH:mm", CultureInfo.CurrentCulture);
            }
        }

        public IEnumerable<WebArticle> Scrape()
        {
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            string html = wc.DownloadString(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var urlPrefix = "http://" + new Uri(url).Host;

            foreach (var elmArticle in doc.DocumentNode.SelectNodes(@"//div[@class=""tack""]"))
            {
                var link = elmArticle.SelectSingleNode(@".//h2/a");
                var content = elmArticle.SelectSingleNode(@".//div[@class=""content""]");
                var updated = elmArticle.SelectSingleNode(@".//div[@class=""updated""]");

                if (link == null || content == null)
                {
                    continue;
                }

                var articleUrl = link.Attributes["href"].Value;

                if (!articleUrl.Contains("showad"))
                {
                    continue;
                }

                articleUrl = urlPrefix + articleUrl;

                var labels =
                    elmArticle
                        .SelectNodes(@".//a|.//span")
                        .Where(a => a.Attributes.Contains("class") && a.Attributes["class"].Value.Contains("label"))
                        .ToList();

                var profile =
                    labels.SingleOrDefault(l => l.Attributes.Contains("class") && l.Attributes["class"].Value.Contains("label-female"));

                var defaultLabels =
                    labels.Where(l => l.Attributes.Contains("class") && l.Attributes["class"].Value.Contains("label-default"))
                    .ToList();

                HtmlNode region;

                if (defaultLabels.Count == 0)
                {
                    region = null;
                }
                else if (defaultLabels.Count == 1)
                {
                    region = defaultLabels.Single();
                }
                else
                {
                    region = defaultLabels.SingleOrDefault(l => l.Attributes.Contains("href") && l.Attributes["href"].Value.Contains("region"));
                }

                var tags = new List<string>();

                if (profile != null)
                {
                    tags.Add("AL_User:" + profile.InnerText.Trim());
                }

                if (region != null)
                {
                    tags.Add("AL_Region:" + region.InnerText.Trim());
                }

                yield return new WebArticle
                {
                    Updated = updated == null ? DateTime.MinValue : ParseUpdated(updated.InnerText.Trim()),
                    Text = content.InnerText,
                    Title = link.InnerText,
                    Url = articleUrl,
                    Tags = tags
                };
            }
        }
    }
}
