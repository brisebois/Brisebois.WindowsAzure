using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Brisebois.WindowsAzure.TableStorage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
    /// <summary>
    ///     Details: http://alexandrebrisebois.wordpress.com/2013/02/20/windows-azure-blob-storage-polling-task/
    /// </summary>
    public class BlobCompressor : BlobContainerWorker
    {
        private const string CompressedFlag = "Compressed";

        /// <summary>
        ///     Keep Blobs compressed within a specific container
        /// </summary>
        /// <param name="connectionString">Cloud Connection String</param>
        /// <param name="containerName">Blob Container Name</param>
        public BlobCompressor(string connectionString, string containerName)
            : base(connectionString, containerName, false)
        {
        }

        protected override void Report(string message)
        {
            Logger.Add("BlobCompressor", "Event", message);
        }

        protected override void OnExecuting(CloudBlockBlob workItem)
        {
            if (workItem == null)
                throw new ArgumentNullException("workItem");

            if (workItem.Metadata.ContainsKey(CompressedFlag))
                return;

            using (var blobStream = new MemoryStream())
            {
                workItem.DownloadToStream(blobStream);

                using (var compressedStream = new MemoryStream())
                {
                    CompressStream(compressedStream, blobStream);

                    SetCompressedFlag(workItem);

                    workItem.UploadFromStream(compressedStream);
                }
            }
        }

        protected override ICollection<IListBlobItem> TryGetWork()
        {
            return base.TryGetWork()
                .Cast<CloudBlockBlob>()
                .Where(b => !b.Metadata.ContainsKey(CompressedFlag))
                .Cast<IListBlobItem>()
                .ToList();
        }

        private static void SetCompressedFlag(CloudBlockBlob workItem)
        {
            workItem.Metadata.Add(new KeyValuePair<string, string>(CompressedFlag,
                "true"));
            workItem.SetMetadata();
        }

        protected static void CompressStream(Stream compressedStream,
            Stream blobStream)
        {
            if (compressedStream == null)
                throw new ArgumentNullException("blobStream");
            if (blobStream == null)
                throw new ArgumentNullException("blobStream");

            blobStream.Position = 0;
            using (GZipStream compressionStream = MakeCompressionStream(compressedStream))
            {
                blobStream.CopyTo(compressionStream);
                compressedStream.Position = 0;
            }
        }

        private static GZipStream MakeCompressionStream(Stream compressedStream)
        {
            return new GZipStream(compressedStream, CompressionLevel.Optimal, true);
        }
    }
}