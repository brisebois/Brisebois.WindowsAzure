using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Brisebois.WindowsAzure.Properties;
using Brisebois.WindowsAzure.TableStorage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
    public class BlobSnapshotCleaner : BlobContainerWorker
    {
        private const int MaxDelayInSeconds = 512;
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
                    false,
                    null,
                    MaxDelayInSeconds)
        {
            this.maxAge = maxAge;
        }

        protected override void Report(string message)
        {
            Logger.Add("BlobSnapshotCleaner", "Event", message);
        }

        protected override ICollection<IListBlobItem> TryGetWork()
        {
            var list = base.TryGetWork();
            return list.Cast<CloudBlockBlob>()
                        .Where(b => b.SnapshotTime.HasValue && IsExpired(b))
                        .Select(b => (IListBlobItem)b)
                        .ToList();
        }

        private bool IsExpired(CloudBlockBlob cloudBlockBlob)
        {
            var snapshotTime = cloudBlockBlob.SnapshotTime.Value.UtcDateTime;
            return DateTime.UtcNow.Subtract(snapshotTime) < maxAge;
        }

        protected override void OnExecuting(CloudBlockBlob workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException("workItem");

            workItem.DeleteIfExists();

            string message = string.Format(CultureInfo.InvariantCulture, Resources.SnapshotCleaner_Deleted_Snapshot_Confirmation, workItem.Uri, workItem.SnapshotTime);
            Report(message);
        }
    }
}