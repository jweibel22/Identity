using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Infrastructure;
using Identity.Infrastructure.Feeders;
using Identity.Infrastructure.Helpers;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Services.NLP;
using log4net;
using Microsoft.Azure.WebJobs;

namespace RssFeederJob
{
    class DirtyUserChannel
    {
        public long UserId { get; set; }

        public long ChannelId { get; set; }
    }

    public class Functions
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string rssFeederUsername = "rssfeeder";

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("refresh-unread-counts")] string message, TextWriter log)
        {
            log.WriteLine("UnreadCounts updater was called. " + message);

            var edges = message.Split(';').Select(s =>
            {
                var x = s.Split('-');
                return new DirtyUserChannel { ChannelId = Int64.Parse(x[0]), UserId = Int64.Parse(x[1]) };
            });

            var connectionFactory =
                new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            try
            {
                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new ChannelLinkRepository(session.Transaction);

                    log.WriteLine("updating unread counts");

                    foreach (var e in edges)
                    {
                        repo.UpdateUnreadCounts(e.UserId, e.ChannelId);
                    }

                    log.WriteLine("Committing session");

                    session.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("UnreadCounts update job failed", ex);
                log.WriteLine("UnreadCounts update job failed. " + ex.Message);
            }
        }

        public static void NewFeederCreated([QueueTrigger("new-feeder-created")] int feederId, TextWriter log)
        {
            _log.Info(feederId);

            var connectionFactory =
                new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            Identity.Domain.User rssFeederUser;
            Feed feed;

            using (var session = connectionFactory.NewTransaction())
            {
                var channelRepo = new ChannelRepository(session.Transaction);
                var userRepo = new UserRepository(session.Transaction);

                rssFeederUser = userRepo.FindByName(rssFeederUsername);
                feed = channelRepo.GetFeedById(feederId);
            }

            if (feed == null)
            {
                _log.Error("Feed with Id " + feederId + " was not found");
                return;
            }

            if (rssFeederUser == null)
            {
                _log.Error("User 'rssFeeder' was not found");
                return;
            }

            var helper = new EnglishLanguage();
            var nlpClient = new GoogleNLPClient(ConfigurationManager.AppSettings["GoogleApiKey"], "https://language.googleapis.com/v1/documents:analyzeEntities", helper);

            var feedRefresher = new FeedRefresher(connectionFactory, log, helper, nlpClient);
            try
            {
                Console.WriteLine("Rss feeder started");
                feedRefresher.Run(rssFeederUser, new[] {feed});
                Console.WriteLine("Rss feeder finished");
            }
            catch (Exception ex)
            {
                _log.Error("RSS feeder failed", ex);
            }
        }


    }
}