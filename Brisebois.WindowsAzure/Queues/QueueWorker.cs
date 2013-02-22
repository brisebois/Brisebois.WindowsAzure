using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Brisebois.WindowsAzure.Queues
{
    public abstract class QueueWorker : PollingTask<CloudQueueMessage>
    {
        private readonly CloudQueueClient client;
        private readonly CloudQueue poisonQueue;
        private readonly CloudQueue queue;
        private readonly TimeSpan visibilityTimeout;
        private readonly int maxAttempts;

        protected QueueWorker(string connectionString,
                              string queueName,
                              string poisonQueueName,
                              int maxAttempts = 10,
                              int visibilityTimeoutInMinutes = 10)
        {
            this.maxAttempts = maxAttempts;

            var cs = CloudConfigurationManager.GetSetting(connectionString);
            var account = CloudStorageAccount.Parse(cs);

            client = account.CreateCloudQueueClient();

            client.RetryPolicy = new ExponentialRetry(new TimeSpan(0, 0, 0, 2), 10);

            queue = client.GetQueueReference(queueName);
            queue.CreateIfNotExists();

            poisonQueue = client.GetQueueReference(poisonQueueName);
            poisonQueue.CreateIfNotExists();

            visibilityTimeout = TimeSpan.FromMinutes(visibilityTimeoutInMinutes);
        }

        protected override void Execute(CloudQueueMessage workItem)
        {
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
            catch (Exception ex)
            {
                Report(ex.ToString());
            }
        }

        protected override ICollection<CloudQueueMessage> GetWork()
        {
            return queue.GetMessages(32, visibilityTimeout)
                        .ToList();
        }
    }
}