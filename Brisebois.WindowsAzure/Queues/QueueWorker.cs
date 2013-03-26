using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Brisebois.WindowsAzure.Queues
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/19/windows-azure-queue-storage-service-polling-task/
    /// </summary>
    public abstract class QueueWorker : PollingTask<CloudQueueMessage>
    {
        private readonly CloudQueueClient client;
        private readonly CloudQueue poisonQueue;
        private readonly CloudQueue queue;
        private readonly TimeSpan visibilityTimeout;
        private readonly int maxAttempts;
        
        protected QueueWorker(string connectionString,
                              string queueName,
                              string poisonQueueName)
            : this(connectionString,queueName,poisonQueueName, 10,10)
        {
        }
        
        protected QueueWorker(string connectionString,
                              string queueName,
                              string poisonQueueName,
                              int maxAttempts,
                              int visibilityTimeoutInMinutes)
        {
            this.maxAttempts = maxAttempts;

            var cs = CloudConfigurationManager.GetSetting(connectionString);
            var account = CloudStorageAccount.Parse(cs);

            client = account.CreateCloudQueueClient();

            ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;

            client.RetryPolicy = new ExponentialRetry(new TimeSpan(0, 0, 0, 2), 10);

            queue = client.GetQueueReference(queueName);

            queue.CreateIfNotExists();

            poisonQueue = client.GetQueueReference(poisonQueueName);
            poisonQueue.CreateIfNotExists();

            visibilityTimeout = TimeSpan.FromMinutes(visibilityTimeoutInMinutes);
        }

        protected override void Execute(CloudQueueMessage workItem)
        {
            if(workItem == null)
                throw new ArgumentNullException("workItem");

            if (workItem.DequeueCount > maxAttempts)
            {
                PlaceMessageOnPoisonQueue(workItem);
                return;
            }

            OnExecuting(workItem);
        }

        protected abstract void OnExecuting(CloudQueueMessage workItem);

        private void PlaceMessageOnPoisonQueue(CloudQueueMessage workItem)
        {
            var message = new CloudQueueMessage(workItem.AsString);
            poisonQueue.AddMessage(message);
            Completed(workItem);
        }

        protected override void Completed(CloudQueueMessage workItem)
        {
            try
            {
                queue.DeleteMessage(workItem);
            }
            catch (StorageException ex)
            {
                Report(ex.ToString());
            }
        }

        protected override ICollection<CloudQueueMessage> TryGetWork()
        {
            return queue.GetMessages(32, visibilityTimeout)
                        .ToList();
        }
    }
}