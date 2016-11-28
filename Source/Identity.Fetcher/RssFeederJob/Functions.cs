using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Identity.Infrastructure;
using Identity.Infrastructure.Repositories;
using log4net;
using Microsoft.Azure.WebJobs;

namespace RssFeederJob
{
    public class Functions
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                    //var events = repo.AllChannelIsDirtyEvents().ToList();

                    //if (!events.Any())
                    //{
                    //    return;
                    //}

                    var channelLinkGraph = repo.GetGraph();

                    foreach (var e in channelIds)
                    {
                        channelLinkGraph.MarkAsDirty(e);
                    }

                    foreach (var edge in channelLinkGraph.DirtyUserChannels)
                    {
                        repo.UpdateUnreadCounts(edge);
                    }

                    //repo.ClearChannelIsDirtyEvents(events.Select(e => e.Id).Max());

                    session.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error("UnreadCounts update job failed", ex);
                log.WriteLine("UnreadCounts update job failed. " + ex.Message);
            }
        }
    }
}
