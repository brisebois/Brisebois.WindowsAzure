using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class TableStorageBulkWorker
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable tableReference;

        private readonly ConcurrentDictionary<ITableEntity, TableOperation> operations;

        public TableStorageBulkWorker(string tableName)
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            tableClient = storageAccount.CreateCloudTableClient();
            tableReference = tableClient.GetTableReference(tableName);
            tableReference.CreateIfNotExists();

            operations = new ConcurrentDictionary<ITableEntity, TableOperation>();
        }

        public ICollection<TEntity> Execute<TEntity>(CloudTableQuery<TEntity> query)
        {
            if(query == null)
                throw new ArgumentNullException("query");

            return query.Execute(tableReference);
        }

        public decimal OutstandingOperations
        {
            get { return operations.Count; }
        }

        public void Insert(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.Insert(entity));
        }

        public void Delete(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.Delete(entity));
        }

        public void InsertOrMerge(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.InsertOrMerge(entity));
        }

        public void InsertOrReplace(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.InsertOrReplace(entity));
        }

        public void Merge(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.Merge(entity));
        }

        public void Replace(ITableEntity entity)
        {
            operations.TryAdd(entity, TableOperation.Replace(entity));
        }

        public void Execute()
        {
            operations
                .GroupBy(kv => kv.Key.PartitionKey)
                .ToList()
                .ForEach(g =>
                    {
                        var opreations = g.ToList();

                        var operationsToExecute = opreations
                            .Take(100)
                            .ToList();
                        var batch = 0;
                        while (operationsToExecute.Any())
                        {
                            var tableBatchOperation = MakeTableBatchOperation(operationsToExecute);

                            ExecuteBatchWithRetries(tableBatchOperation);

                            RemoveOperations(operationsToExecute);

                            batch++;
                            operationsToExecute = GetOperationsToExecute(opreations, batch);
                        }
                    });
        }

        private void ExecuteBatchWithRetries(TableBatchOperation tableBatchOperation)
        {
            var tableRequestOptions = MakeTableRequestOptions();
            tableReference.ExecuteBatch(tableBatchOperation, tableRequestOptions);
        }

        private static TableRequestOptions MakeTableRequestOptions()
        {
            return new TableRequestOptions
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(2), 100)
                };
        }

        private static TableBatchOperation MakeTableBatchOperation(List<KeyValuePair<ITableEntity, TableOperation>> operationsToExecute)
        {
            var tableBatchOperation = new TableBatchOperation();
            operationsToExecute.ForEach(kv => tableBatchOperation.Add(kv.Value));
            return tableBatchOperation;
        }

        private static List<KeyValuePair<ITableEntity, TableOperation>> GetOperationsToExecute(IEnumerable<KeyValuePair<ITableEntity, TableOperation>> opreations,
                                                                                               int batch)
        {
            return opreations
                .Skip(batch * 100)
                .Take(100)
                .ToList();
        }

        private void RemoveOperations(List<KeyValuePair<ITableEntity, TableOperation>> operationsToExecute)
        {
            operationsToExecute.ForEach(kv =>
                {
                    TableOperation opretation;
                    operations.TryRemove(kv.Key, out opretation);
                });
        }
    }
}