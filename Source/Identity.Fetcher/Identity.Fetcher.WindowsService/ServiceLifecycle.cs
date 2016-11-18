using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using Identity.Domain;
using Identity.Infrastructure;
using Identity.Infrastructure.Feeders;
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
        private const string rssFeederUsername = "rssfeeder";
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

            //var items = new TwitterFeeder().Fetch();


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
                log.Error("User 'rssFeeder' was not found");
                return;
            }

            var feedRefresher = new RssFeedRefresher(connectionFactory);
            try
            {
                feedRefresher.Run(rssFeederUser, feeders);
            }
            catch (Exception ex)
            {
                log.Error("RSS feeder failed", ex);
            }

            //var webScraperJob = new WebScraperJob(connectionFactory);
            //try
            //{
            //    webScraperJob.Run();
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Web scraper job failed", ex);
            //}

            //var refresher = new TagCloudRefresher(connectionFactory);
            //try
            //{
            //    refresher.Execute();
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Tag cloud refresher failed", ex);
            //}


            //try
            //{

            //    var connectionFactory = new ConnectionFactory(ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString);
            //    //var feedRefresher = new RssFeedRefresher(connectionFactory);
            //    //feedRefresher.Run();

            //    var webscraperJob = new WebScraperJob(connectionFactory);
            //    webscraperJob.Run();


            //    //using (
            //    //    var con =
            //    //        new SqlConnection(
            //    //            ConfigurationManager.ConnectionStrings["Sql.ConnectionString"].ConnectionString))
            //    //{
            //    //    con.Open();
            //    //    using (var transaction = con.BeginTransaction())
            //    //    {
            //    //        //var channelRepo = new ChannelRepository(transaction);
            //    //        //var postRepo = new PostRepository(transaction);

            //    //        //var refresher = new TagCloudRefresher(channelRepo);
            //    //        //refresher.Execute();

            //    //    //    transaction.Commit();
            //    //    }
            //    //}
            //}
            //catch (Exception ex)
            //{
            //    log.Error(ex);
            //}
        }
    }
}
