using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain;
using Identity.Domain.Clustering;
using Identity.Infrastructure;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Rss;
using log4net;
using Microsoft.Azure.WebJobs;
using User = Identity.Infrastructure.DTO.User;

namespace FeederJob2
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

        public static void RefreshOntologies()
        {
            try
            {
                Console.WriteLine("Ontology refresh started");

                var ontologyId = 1;
                var commonWords = System.IO.File.ReadAllLines(@"commonwords-danish.txt").Select(w => w.Trim()).ToArray();
                var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
                IList<Document> articles;

                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new OntologyRepository(session.Transaction);

                    articles = repo.GetNextClusteringWindow(ontologyId);
                }                               

                var clusters = Algorithm.ComputeClusters(commonWords, articles);

                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new OntologyRepository(session.Transaction);

                    repo.UpdateClusters(ontologyId, clusters);

                    session.Commit();
                }

                Console.WriteLine("Ontology refresh finished");
            }
            catch (Exception ex)
            {
                logger.Error("Ontology refresh failed", ex);
                throw;
            }




        }
    }
}
