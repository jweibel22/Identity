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
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Run()
        {            
            XmlConfigurator.Configure();

            //var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);

            //var feedRefresher = new RssFeedRefresher(connectionFactory);
            //try
            //{
            //    Console.WriteLine("Rss feeder started");
            //    feedRefresher.Run();
            //    Console.WriteLine("Rss feeder finished");
            //}
            //catch (Exception ex)
            //{
            //    log.Error("RSS feeder failed", ex);
            //}           

            //var webScraperJob = new WebScraperJob(connectionFactory);
            //try
            //{
            //    Console.WriteLine("Web scraper started");
            //    webScraperJob.Run();
            //    Console.WriteLine("Web scraper finished");
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Web scraper job failed", ex);
            //}

            //var refresher = new TagCloudRefresher(connectionFactory);
            //try
            //{
            //    Console.WriteLine("tag cloud refresher started");
            //    refresher.Execute();
            //    Console.WriteLine("tag cloud refresher finished");
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Tag cloud refresher failed", ex);
            //}
        }
    }
}
