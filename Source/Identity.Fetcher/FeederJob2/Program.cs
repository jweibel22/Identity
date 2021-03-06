﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.Azure.WebJobs;

namespace FeederJob2
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            XmlConfigurator.Configure();

            //var host = new JobHost();            
            //host.RunAndBlock();

            //Functions.ProcessQueueMessage("5", TextWriter.Null);

            Functions.SyncFeeds("", TextWriter.Null);
            Functions.ReloadOntology();

            if (DateTime.Now.Hour == 0)
            {
                Functions.RefreshChannelScores();
            }

            //NlpTest.Run();
        }
    }
}
