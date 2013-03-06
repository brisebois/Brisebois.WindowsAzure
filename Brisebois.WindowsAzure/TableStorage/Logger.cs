using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/08/service-logger-for-windows-azure-roles-using-table-storage-service/
    /// </summary>
    public static class Logger
    {
        private const string TableName = "RoleLogTable";
        private static readonly TableStorageWriter Worker = new TableStorageWriter(TableName);

        private static DateTime lastPersist;
        private static readonly object LockObject = new object();

        public static async Task<Entry[]> Get(int count, IEnumerable<string> partitions)
        {
            var tasks = partitions
                .Select(p => MakeTaskForPartition(p, count))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            return results.ToList().SelectMany(t => t).ToArray();
        }

        private static Task<ICollection<Entry>> MakeTaskForPartition(string partition, int count)
        {
            return Task.Run(() =>
                {
                    var reader = new TableStorageReader(TableName);
                    var query = new GetTopLogEntriesForPartition<Entry>(partition, count);
                    return reader.Execute(query);
                });
        }

        private static bool isPersisting;

        public static void Persist(bool force)
        {
            var delay = DateTime.UtcNow.Subtract(lastPersist);

            if (!force)
                if (isPersisting
                    && Worker.OutstandingOperations <= 100
                    && !(delay.TotalSeconds > 20))
                    return;

            lock (LockObject)
            {
                if (Worker.OutstandingOperations == 0)
                    return;

                isPersisting = true;

                Worker.Execute();

                lastPersist = DateTime.UtcNow;

                isPersisting = false;
            }
        }

        public static void Add(string service, string @event)
        {
            Add(service, @event, string.Empty);
        }

        public static void Add(string service, string @event, string details)
        {
            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{3}{0}\t{1}{3}{2}{3}", service, @event, details, Environment.NewLine));

            Worker.InsertOrMerge(Entry.Make(service, @event, details));
            Persist(false);
        }
    }
}