using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Rss;
using Identity.Infrastructure.Services;
using log4net;
using log4net.Config;

namespace RssFeeder
{
    class ServiceLifecycle
    {
        private Timer timer;
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Start()
        {
            XmlConfigurator.Configure();

            timer = new Timer(Run, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            log.Info("Service started");
        }

        public void Stop()
        {
            timer.Dispose();
            log.Info("Service stopped");
        }

        private void Run(object state)
        {
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    var channelRepo = new ChannelRepository(transaction);
                    var postRepo = new PostRepository(transaction);
                    var userRepo = new UserRepository(transaction);

                    var feedRefresher = new RssFeedRefresher(postRepo, userRepo, channelRepo);

                    feedRefresher.Run();

                    transaction.Commit();
                }
            }
        }
    }
}
