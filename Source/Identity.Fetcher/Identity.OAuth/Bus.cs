using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Identity.OAuth
{
    public class Bus
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudQueueClient queueClient;

        public Bus()
        {
            // CloudConfigurationManager.GetSetting("AzureWebJobsDashboard")
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsDashboard"].ConnectionString);
            queueClient = storageAccount.CreateCloudQueueClient();
        }

        public void Publish(string queueName, string message)
        {                        
            var queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();            
            queue.AddMessage(new CloudQueueMessage(message));
        }
    }
}