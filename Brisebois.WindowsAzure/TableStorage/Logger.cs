using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace Brisebois.WindowsAzure.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/08/service-logger-for-windows-azure-roles-using-table-storage-service/
    /// </summary>
    public static class Logger
    {
        static Logger()
        {
            TableName = "RoleLogTable";
            Init();
        }

        public static void Init()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudStorageAccount.CreateCloudTableClient().GetTableReference(TableName).CreateIfNotExists();
        }

        public async static Task<List<Entry>> Get(int count, IEnumerable<string> partitions)
        {
            var tasks = partitions
                .Select(p => MakeTaskForPartition(count, p))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            return results.ToList().SelectMany(t => t).ToList();
        }

        private static Task<List<Entry>> MakeTaskForPartition(int count, string partition)
        {
            return Task.Run(() =>
                {
                    using (var tableClient = MakeTableClient())
                    {
                        return
                            tableClient.CreateQuery<Entry>(TableName)
                                       .Where(e => e.PartitionKey == partition)
                                       .Take(count)
                                       .ToList();
                    }
                });
        }

        private static CloudStorageAccount cloudStorageAccount;

        public static readonly ConcurrentBag<Entry> Entries = new ConcurrentBag<Entry>();
        private static readonly string TableName;

        private static DateTime lastPersist;
        private static readonly object LockObject = new object();

        public static void Persist(bool force)
        {
            var delay = DateTime.UtcNow.Subtract(lastPersist);
            if (force || Entries.Count > 100 || delay.TotalSeconds > 20)
                TryPersist();
        }

        private static void TryPersist()
        {
            Task.Run(() =>
                {
                    lock (LockObject)
                    {
                        lastPersist = DateTime.UtcNow;
                    }

                    var count = Entries.Count;


                    for (var c = 0; c < count; )
                    {
                        var newLogs = GetOneHundredEntries();
                        PersistEntriesByPartition(newLogs);

                        c = c + 100;
                    }
                });
        }

        private static void PersistEntriesByPartition(IEnumerable<Entry> entries)
        {
            entries.GroupBy(l => l.PartitionKey)
                   .ToList()
                   .AsParallel()
                   .ForAll(entities =>
                       {
                           using (var tableClient = MakeTableClient())
                           {
                               foreach (var entity in entities)
                               {
                                   tableClient.AddObject(TableName, entity);
                               }
                               SaveBatchWithRetries(tableClient);
                           }
                       });
        }

        private static IEnumerable<Entry> GetOneHundredEntries()
        {
            var entries = new List<Entry>();
            Enumerable.Range(0, 99).ToList().ForEach(index =>
                {
                    Entry log;
                    Entries.TryTake(out log);
                    if (log != null)
                        entries.Add(log);
                });
            return entries;
        }

        private static void SaveBatchWithRetries(TableServiceContext client)
        {
            var tableRequestOptions = new TableRequestOptions
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 10)
                };
            client.SaveChangesWithRetries(SaveChangesOptions.Batch,
                                          tableRequestOptions);
        }

        private static TableServiceContext MakeTableClient()
        {
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            return cloudTableClient.GetTableServiceContext();
        }

        public static void Add(string service, string @event, string details = "")
        {
            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format("{3}{0}\t{1}{3}{2}{3}", service, @event, details, Environment.NewLine));

            Entries.Add(new Entry(service, @event, details));
            Persist(false);
        }
    }
}