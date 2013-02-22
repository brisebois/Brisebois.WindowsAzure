using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
    public class BlocCompressor : BlobContainerWorker
    {
        private const string CompressedFlag = "Compressed";

        public BlocCompressor(string connectionString, string containerName)
            : base(connectionString, containerName, false)
        {
        }

        protected override void Report(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnExecuting(CloudBlockBlob workItem)
        {
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

        protected override ICollection<IListBlobItem> GetWork()
        {
            return base.GetWork()
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

        protected void CompressStream(MemoryStream compressedStream,
                                      MemoryStream blobStream)
        {
            blobStream.Position = 0;
            using (var compressionStream = MakeCompressionStream(compressedStream))
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