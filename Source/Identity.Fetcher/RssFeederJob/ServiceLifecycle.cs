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

namespace RssFeeder
{
    class ServiceLifecycle
    {
        //private Timer timer;
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Start()
        {
            XmlConfigurator.Configure();

            //timer = new Timer(Run, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            Console.WriteLine("Service started");
        }

        public void Stop()
        {
            //timer.Dispose();
            Console.WriteLine("Service stopped");
        }

        public void Run()
        {
            var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            var feedRefresher = new RssFeedRefresher(connectionFactory);
            try
            {
                feedRefresher.Run();
            }
            catch (Exception ex)
            {
                log.Error("RSS feeder failed", ex);
            }           

            var webScraperJob = new WebScraperJob(connectionFactory);
            try
            {
                webScraperJob.Run();
            }
            catch (Exception ex)
            {
                log.Error("Web scraper job failed", ex);
            }

            var refresher = new TagCloudRefresher(connectionFactory);
            try
            {
                refresher.Execute();
            }
            catch (Exception ex)
            {
                log.Error("Tag cloud refresher failed", ex);
            }
        }
    }
}
