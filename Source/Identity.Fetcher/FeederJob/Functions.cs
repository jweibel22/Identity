using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Identity.Infrastructure;
using Identity.Infrastructure.Rss;
using log4net;
using Microsoft.Azure.WebJobs;

namespace FeederJob
{
    public class Functions
    {
        private static ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void NewFeederCreated([QueueTrigger("new-feeder-created")] int feederId, TextWriter log)
        {
            logger.Info(feederId);
        }

        public static void SyncFeeds([QueueTrigger("sync-feeds")] string message, TextWriter log)
        {            
            var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            var feedRefresher = new RssFeedRefresher(connectionFactory);
            try
            {
                Console.WriteLine("Rss feeder started");
                feedRefresher.Run();
                Console.WriteLine("Rss feeder finished");
            }
            catch (Exception ex)
            {
                logger.Error("RSS feeder failed", ex);
            }
        }
    }
}
