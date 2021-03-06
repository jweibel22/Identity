﻿using System;
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
using User = Identity.Infrastructure.DTO.User;

namespace FeederJob
{
    public class Functions
    {
        private static ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string rssFeederUsername = "rssfeeder";

        public static void SyncFeeds([QueueTrigger("sync-feeds")] string message, TextWriter log)
        {
            try
            {
                var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

                Identity.Domain.User rssFeederUser;
                IEnumerable<Feed> feeders;

                using (var session = connectionFactory.NewTransaction())
                {
                    var channelRepo = new ChannelRepository(session.Transaction);
                    var userRepo = new UserRepository(session.Transaction);

                    rssFeederUser = userRepo.FindByName(rssFeederUsername);
                    feeders = channelRepo.OutOfSyncFeeds(TimeSpan.FromHours(1));
                }

                if (rssFeederUser == null)
                {
                    logger.Error("User 'rssFeeder' was not found");
                    return;
                }

                var feedRefresher = new RssFeedRefresher(connectionFactory, log);
                Console.WriteLine("Rss feeder started");
                feedRefresher.Run(rssFeederUser, feeders);
                Console.WriteLine("Rss feeder finished");
            }
            catch (Exception ex)
            {
                logger.Error("RSS feeder failed", ex);
                throw;
            }
        }
    }
}
