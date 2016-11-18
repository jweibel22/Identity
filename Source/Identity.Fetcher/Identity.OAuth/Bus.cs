using System;
using System.Collections.Generic;
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
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
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