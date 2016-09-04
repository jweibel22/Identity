using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Identity.Infrastructure.Rss;
using log4net;
using log4net.Core;
using LinqToTwitter;

namespace Identity.Infrastructure.Feeders
{
    public class TwitterFeeder : IFeederReader
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string accountName;

        public TwitterFeeder(string accountName)
        {
            this.accountName = accountName;
        }

        private IEnumerable<string> SubstringsStartingWith(string s, IEnumerable<string> startsWith)
        {
            return startsWith.SelectMany(sw => SubstringsStartingWith(s, sw));
        }

        private IEnumerable<string> SubstringsStartingWith(string s, string startsWith)
        {
            var reg = new Regex(String.Format(@"{0}[\w\.\/]+", startsWith));
            var matches = reg.Matches(s);

            foreach (Match match in matches)
            {
                yield return match.Value;
            }            
        }

        private string RemoveAll(string s, IEnumerable<string> toRemove)
        {
            var result = s;

            foreach (var x in toRemove)
            {
                result = result.Replace(x, "");
            }

            return result;
        }

        public IEnumerable<FeedItem> Fetch()
        {
            var auth = new ApplicationOnlyAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = "uyr3uEP6Cph7LvCcJKuYIVEMe",
                    ConsumerSecret = "UJbPcahi6H07AlkaDLlNofBvWosHbUkOKJ0RqpRT92u8tk7Dpg"
                }
            };

            auth.AuthorizeAsync().Wait();

            var twitterCtx = new TwitterContext(auth);

            var tweets = 
                from tweet in twitterCtx.Status
                where tweet.Type == StatusType.User &&
                      tweet.ScreenName == accountName &&
                      tweet.Count == 20
                select tweet;

            var x = tweets.ToList();

            return x.Select(i => new FeedItem
            {
                CreatedAt = i.CreatedAt,
                Title = RemoveAll(i.Text, SubstringsStartingWith(i.Text, new [] { "@", "http://", "https://" })).Trim(),
                Content = "",
                Links = i.Entities.UrlEntities.Select(e => new Uri(e.Url)),
                Tags = i.Entities.HashTagEntities.Select(e => e.Tag)
            });
        }
    }
}
