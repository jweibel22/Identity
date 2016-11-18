using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.WebJobs;

namespace FeederJob
{
    public class Functions
    {
        private static ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void ProcessQueueMessage([QueueTrigger("new-feeder-created")] int feederId, TextWriter log)
        {
            //log.WriteLine(message);

            logger.Info(feederId);
        }
    }
}
