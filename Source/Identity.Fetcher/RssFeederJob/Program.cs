using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net.Config;
using Microsoft.Azure.WebJobs;
using RssFeeder;

namespace RssFeederJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            XmlConfigurator.Configure();

            var host = new JobHost();            
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();

            //new ServiceLifecycle().Run();
        }
    }
}
