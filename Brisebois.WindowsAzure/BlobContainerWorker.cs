using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Brisebois.WindowsAzure
{
    public class TestMessageQueueWorker : QueueWorker
    {
        public TestMessageQueueWorker(string connectionString,
                                  string queueName,
                                  string poisonQueueName,
                                  int maxAttempts = 10,
                                  int visibilityTimeOutInMinutes = 10)
            : base(connectionString,
                   queueName,
                   poisonQueueName,
                   maxAttempts,
                   visibilityTimeOutInMinutes)
        {
        }

        protected override void Report(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnExecuting(CloudQueueMessage workItem)
        {
            //Do some work 
            var message = workItem.AsString;
            Trace.WriteLine(message);

            //Used for testing the poison queue
            if (message == "fail")
                throw new Exception(message);

            Task.Delay(TimeSpan.FromSeconds(10))
                .Wait();
        }
    }

    public abstract class BlobContainerWorker : PollingTask<IListBlobItem>
    {
        private readonly bool deleteBlobOnCompleted;
        protected readonly CloudBlobClient client;
        protected readonly CloudBlobContainer container;
        private readonly int? batchSize;

        protected BlobContainerWorker(string connectionString,
                                      string containerName,
                                      bool deleteBlobOnCompleted = true,
                                      int? batchSize = null)
        {
            this.deleteBlobOnCompleted = deleteBlobOnCompleted;
            this.batchSize = batchSize;
            var cs = CloudConfigurationManager.GetSetting(connectionString);
            var account = CloudStorageAccount.Parse(cs);

            client = account.CreateCloudBlobClient();

            var deltaBackoff = new TimeSpan(0, 0, 0, 2);
            client.RetryPolicy = new ExponentialRetry(deltaBackoff, 10);

            container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

        }

        protected override void Completed(IListBlobItem workItem)
        {
            if (deleteBlobOnCompleted)
            {
                var blob = workItem as CloudBlockBlob;
                if (blob != null)
                    blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
            }
        }

        protected abstract void OnExecuting(CloudBlockBlob workItem);

        protected override void Execute(IListBlobItem workItem)
        {
            var blob = workItem as CloudBlockBlob;
            if (blob == null)
                return;

            OnExecuting(blob);
        }

        protected override ICollection<IListBlobItem> GetWork()
        {
            if (batchSize.HasValue)
                return container.ListBlobs("", true, BlobListingDetails.Metadata)
                                .Take(batchSize.Value)
                                .ToList();

            return container.ListBlobs("", true, BlobListingDetails.Metadata)
                            .ToList();
        }
    }
}