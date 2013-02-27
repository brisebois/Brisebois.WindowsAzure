using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
    public class BlobSnapshotCleaner : BlobContainerWorker
    {
        private readonly TimeSpan maxAge;

        /// <summary>
        /// Keep Snapshots for a specific amount of time
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        /// <param name="maxAge">MAX Age for a Blob Snapshot</param>
        public BlobSnapshotCleaner(string connectionString,
                                   string containerName,
                                   TimeSpan maxAge)
            : base(connectionString,
                    containerName,
                    BlobListingDetails.Snapshots,
                    false)
        {
            this.maxAge = maxAge;
        }

        protected override void Report(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnExecuting(CloudBlockBlob workItem)
        {
            if (!workItem.SnapshotTime.HasValue) return;

            var snapshotTime = workItem.SnapshotTime.Value.UtcDateTime;
            if (DateTime.UtcNow.Subtract(snapshotTime) > maxAge)

                workItem.DeleteIfExists();

            Report(string.Format("DELETED {0} Snapshot Time {1}",
                                    workItem.Uri,
                                    workItem.SnapshotTime));
        }
    }
}