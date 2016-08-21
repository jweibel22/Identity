using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using Identity.Infrastructure;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Rss;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.WebScrapers;
using log4net;
using log4net.Config;

namespace Identity.Fetcher.WindowsService
{
    class ServiceLifecycle
    {
        private Timer timer;
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Start()
        {
            XmlConfigurator.Configure();

            //WriteRss();

            timer = new Timer(Run, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            log.Info("Service started");
        }

        public void Stop()
        {
            timer.Dispose();
            log.Info("Service stopped");
        }

        private void Run(object state)
        {
            try
            {

                var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
                //var feedRefresher = new RssFeedRefresher(connectionFactory);
                //feedRefresher.Run();

                var webscraperJob = new WebScraperJob(connectionFactory);
                webscraperJob.Run();


                //using (
                //    var con =
                //        new SqlConnection(
                //            ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString))
                //{
                //    con.Open();
                //    using (var transaction = con.BeginTransaction())
                //    {
                //        //var channelRepo = new ChannelRepository(transaction);
                //        //var postRepo = new PostRepository(transaction);

                //        //var refresher = new TagCloudRefresher(channelRepo);
                //        //refresher.Execute();

                //    //    transaction.Commit();
                //    }
                //}
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
