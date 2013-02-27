﻿using System;
using System.Collections.Generic;
using System.Linq;
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
            Console.WriteLine(message);
        }

        protected override ICollection<IListBlobItem> GetWork()
        {
            var list = base.GetWork();
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
            workItem.DeleteIfExists();

            Report(string.Format("DELETED {0} Snapshot Time {1}",
                                    workItem.Uri,
                                    workItem.SnapshotTime));
        }
    }
}