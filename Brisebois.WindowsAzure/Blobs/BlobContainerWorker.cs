using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Brisebois.WindowsAzure.Blobs
{
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