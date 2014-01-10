using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Storage.Blobs
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/20/windows-azure-blob-storage-polling-task/
    /// </summary>
    public abstract class BlobContainerWorker
        : PollingTask<IListBlobItem>
    {
        private readonly bool deleteBlobOnCompleted;

        private readonly int? batchSize;
        private readonly BlobListingDetails blobListnigDetails = BlobListingDetails.Metadata;

        protected CloudBlobClient Client { get; private set; }
        protected CloudBlobContainer Container { get; private set; }

        /// <summary>
        /// Will list blobs based on your configurations
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="listingDetails">BlobListingDetails specify the type of informations that is returned by the TryGetWork method</param>
        /// <param name="deleteBlobOnCompleted">specifies is a bloob needs to be deleted once it has been treated</param>
        /// <param name="batchSize">Number of blobs to read from the top of the container</param>
        ///  /// <param name="maxDelayInSeconds">Set the maximum Backoff delay between attemps to read data from the source</param>
        protected BlobContainerWorker(string connectionString,
                                      string containerName,
                                      BlobListingDetails listingDetails,
                                      bool deleteBlobOnCompleted,
                                      int? batchSize,
                                      int maxDelayInSeconds)
            : this(connectionString, containerName, deleteBlobOnCompleted, batchSize, maxDelayInSeconds)
        {
            blobListnigDetails = listingDetails;
        }


        /// <summary>
        /// Will list Blobs with thier metatadata
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="deleteBlobOnCompleted">specifies is a bloob needs to be deleted once it has been treated</param>
        /// <param name="batchSize">Number of blobs to read from the top of the container</param>
        /// <param name="maxDelayInSeconds">Set the maximum Backoff delay between attemps to read data from the source</param>
        protected BlobContainerWorker(string connectionString,
                                      string containerName,
                                      bool deleteBlobOnCompleted,
                                      int? batchSize,
                                      int maxDelayInSeconds)
            : base(maxDelayInSeconds)
        {
            this.deleteBlobOnCompleted = deleteBlobOnCompleted;
            this.batchSize = batchSize;
            var cs = CloudConfigurationManager.GetSetting(connectionString);
            var account = CloudStorageAccount.Parse(cs);

            Client = account.CreateCloudBlobClient();

            Container = Client.GetContainerReference(containerName);
            Container.CreateIfNotExists();

        }

        /// <summary>
        /// Will list the full list of Blobs including thier metadata and has a max delay of 1024 seconds
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="deleteBlobOnCompleted">specifies is a bloob needs to be deleted once it has been treated</param>
        protected BlobContainerWorker(string connectionString, string containerName, bool deleteBlobOnCompleted)
            : this(connectionString, containerName, deleteBlobOnCompleted, null, 1024)
        {

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

        protected override ICollection<IListBlobItem> TryGetWork()
        {
            if (batchSize.HasValue)
                return Container.ListBlobs("", true, blobListnigDetails)
                                .Take(batchSize.Value)
                                .ToList();

            return Container.ListBlobs("", true, blobListnigDetails)
                            .ToList();
        }
    }
}