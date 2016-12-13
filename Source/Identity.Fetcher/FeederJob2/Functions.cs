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

        private const double threshold = 3;
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


        private static List<Cluster> RebuildCluster(IList<Document> articles, IList<PostClusterMember> conf)
        {
            var clusters = conf.Select(c => c.ClusterId).Distinct().ToDictionary(id => id, id => new List<Document>());
            var xx = new List<List<Document>>();

            foreach (var article in articles)
            {
                var member = conf.SingleOrDefault(c => c.PostId == article.Id);

                if (member != null)
                {
                    clusters[member.ClusterId].Add(article);
                }
                else
                {
                    xx.Add(new[] { article }.ToList());
                }
            }            

            return clusters.Values.ToList().Union(xx).Select(c => new Cluster(c)).ToList();
        }

        public static void ReloadOntology()
        {
            try
            {
                Console.WriteLine("Ontology reload started");

                var ontologyId = 1;
                var commonWords = System.IO.File.ReadAllLines(@"commonwords-danish.txt").Select(w => w.Trim()).ToArray();
                var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
                IList<Document> articles;
                IEnumerable<Document> newArticles;
                List<Cluster> clusters;

                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new OntologyRepository(session.Transaction);

                    var ontology = repo.GetOntology(ontologyId);
                    var reset = DateTime.Now.Date > ontology.Updated.Date;
                    var from = DateTime.Now.Date.Subtract(TimeSpan.FromDays(7));                    
                    articles = repo.GetPostsFromOntology(ontologyId, from);
                    Algorithm.CalculateWordVectors(commonWords, articles);

                    if (reset)
                    {
                        clusters = new List<Cluster>();
                        newArticles = articles;
                    }
                    else
                    {
                        var conf = repo.GetClusterMembers(ontologyId);
                        var alreadyProcessedArticles = articles.Where(a => a.Added < ontology.Updated);
                        clusters = RebuildCluster(alreadyProcessedArticles.ToList(), conf);
                        newArticles = articles.Where(a => a.Added >= ontology.Updated);
                    }
                }
                
                var world = new World(threshold, clusters);

                foreach (var article in newArticles)
                {
                    world.Add(article);
                }

                var nonTrivialClusters = world.Clusters.Where(c => c.Documents.Count > 1).ToList();

                using (var session = connectionFactory.NewTransaction())
                {
                    var repo = new OntologyRepository(session.Transaction);

                    repo.UpdateClusters(ontologyId, nonTrivialClusters);

                    session.Commit();
                }

                Console.WriteLine("Ontology reload finished");
            }
            catch (Exception ex)
            {
                logger.Error("Ontology reload failed", ex);
                throw;
            }

        }        
    }
}
