using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Brisebois.WindowsAzure.Blobs
{
    public class ShardedBlobContainer
    {
        private readonly string name;

        private readonly List<CloudBlobContainer> containers;
        private readonly int count;
        private int lastLocation;

        public ShardedBlobContainer(string name,
                                    IEnumerable<CloudStorageAccount> storagAccounts)
        {
            this.name = name;

            var accounts = storagAccounts.ToList();

            count = accounts.Count();

            var random = new Random(DateTime.Now.Millisecond);
            lastLocation = random.Next(count);

            var clients = accounts.Select(a => a.CreateCloudBlobClient())
                                    .ToList();

            containers = clients.Select(c => c.GetContainerReference(name))
                                .ToList();
        }

        public string Name
        {
            get { return name; }
        }

        public void CreateIfNotExists()
        {
            containers.AsParallel().ForAll(c => c.CreateIfNotExists());
        }

        public ICancellableAsyncResult Upload(string blobName,
                                                string content,
                                                AsyncCallback callback)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(content);
            writer.Flush();

            return Upload(blobName, stream, callback);
        }

        readonly object lockObject = new object();

        public ICancellableAsyncResult Upload(string blobName,
                                                Stream stream,
                                                AsyncCallback callback)
        {
            lock (lockObject)
                lastLocation = (lastLocation + 1) % count;

            var container = containers[lastLocation];
            var blobReferenceFromServer = container.GetBlockBlobReference(blobName);
            return blobReferenceFromServer
                    .BeginUploadFromStream(stream,
                                            callback,
                                            blobReferenceFromServer);
        }

        public IEnumerable<ICloudBlob> Find(string prefix, bool useFlatBlobListing)
        {
            return containers.AsParallel()
                .SelectMany(c => c.ListBlobs(prefix, useFlatBlobListing))
                .Cast<ICloudBlob>()
                .ToList();
        }
    }
}