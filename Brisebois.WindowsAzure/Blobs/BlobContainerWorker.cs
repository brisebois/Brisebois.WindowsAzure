using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Brisebois.WindowsAzure.Blobs
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/20/windows-azure-blob-storage-polling-task/
    /// </summary>
    public abstract class BlobContainerWorker 
        : PollingTask<IListBlobItem>
    {
        private readonly bool deleteBlobOnCompleted;
        protected readonly CloudBlobClient client;
        protected readonly CloudBlobContainer container;
        private readonly int? batchSize;
        private readonly BlobListingDetails blobListnigDetails = BlobListingDetails.Metadata;

        /// <summary>
        /// A base class used to work with Blob Containers
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="listingDetails">BlobListingDetails specify the type of informations that is returned by the GetWork method</param>
        /// <param name="deleteBlobOnCompleted">specifies is a bloob needs to be deleted once it has been treated</param>
        /// <param name="batchSize">Number of blobs to read from the top of the container</param>
        ///  /// <param name="maxDelayInSeconds">Set the maximum Backoff delay between attemps to read data from the source</param>
        protected BlobContainerWorker(string connectionString,
                                      string containerName,
                                      BlobListingDetails listingDetails,
                                      bool deleteBlobOnCompleted = true,
                                      int? batchSize = null,
                                      int maxDelayInSeconds =  1024)
            : this(connectionString,containerName,deleteBlobOnCompleted,batchSize, maxDelayInSeconds)
        {
            blobListnigDetails = listingDetails;
        }

        /// <summary>
        /// A base class used to work with Blob Containers
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="deleteBlobOnCompleted">specifies is a bloob needs to be deleted once it has been treated</param>
        /// <param name="batchSize">Number of blobs to read from the top of the container</param>
        /// <param name="maxDelayInSeconds">Set the maximum Backoff delay between attemps to read data from the source</param>
        protected BlobContainerWorker(string connectionString,
                                      string containerName,
                                      bool deleteBlobOnCompleted = true,
                                      int? batchSize = null,
                                      int maxDelayInSeconds = 1024)
            : base(maxDelayInSeconds)
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
                return container.ListBlobs("", true, blobListnigDetails)
                                .Take(batchSize.Value)
                                .ToList();

            return container.ListBlobs("", true, blobListnigDetails)
                            .ToList();
        }
    }
}