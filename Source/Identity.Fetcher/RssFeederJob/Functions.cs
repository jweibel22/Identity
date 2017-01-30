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
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Rss;
using log4net;
using Microsoft.Azure.WebJobs;

namespace RssFeederJob
{
    public class Functions
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string rssFeederUsername = "rssfeeder";

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("refresh-unread-counts")] string message, TextWriter log)
        {
            log.WriteLine("UnreadCounts updater was called. " + message);

            var channelIds = message.Split(';').Select(Int64.Parse);

            var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            try
            {
                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new ChannelLinkRepository(session.Transaction);
                    var channelLinkGraph = repo.GetGraph();

                    foreach (var e in channelIds)
                    {
                        channelLinkGraph.MarkAsDirty(e);
                    }

                    foreach (var edge in channelLinkGraph.DirtyUserChannels)
                    {
                        repo.UpdateUnreadCounts(edge);
                    }

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

            var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

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

            var feedRefresher = new FeedRefresher(connectionFactory, log);
            try
            {
                Console.WriteLine("Rss feeder started");
                feedRefresher.Run(rssFeederUser, new[] { feed });
                Console.WriteLine("Rss feeder finished");
            }
            catch (Exception ex)
            {
                _log.Error("RSS feeder failed", ex);
            }
        }

    }
}
