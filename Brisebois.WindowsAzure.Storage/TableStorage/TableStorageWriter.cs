using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/03/06/inserting-modifying-large-amounts-of-data-in-windows-azure-table-storage-service/
    /// </summary>
    public class TableStorageWriter
    {
        private const int BatchSize = 100;
        private readonly ConcurrentQueue<Tuple<ITableEntity, TableOperation>> operations;
        private readonly CloudStorageAccount storageAccount;
        private readonly string tableName;

        public TableStorageWriter(string tableName)
        {
            this.tableName = tableName;

            var cs = CloudConfigurationManager.GetSetting("StorageConnectionString");

            storageAccount = CloudStorageAccount.Parse(cs);

            ServicePointManager.FindServicePoint(storageAccount.TableEndpoint).UseNagleAlgorithm = false;

            operations = new ConcurrentQueue<Tuple<ITableEntity, TableOperation>>();
        }

        public void CreateIfNotExist()
        {
            var tableReference = MakeTableReference();
            tableReference.CreateIfNotExists();
        }

        private CloudTable MakeTableReference()
        {
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference(tableName);
            return tableReference;
        }

        public decimal OutstandingOperations
        {
            get { return operations.Count; }
        }

        public void Insert<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.Insert(entity));
            operations.Enqueue(e);
        }

        public void Delete<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.Delete(entity));
            operations.Enqueue(e);
        }

        public void InsertOrMerge<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.InsertOrMerge(entity));
            operations.Enqueue(e);
        }

        public void InsertOrReplace<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.InsertOrReplace(entity));
            operations.Enqueue(e);
        }

        public void Merge<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.Merge(entity));
            operations.Enqueue(e);
        }

        public void Replace<TEntity>(TEntity entity)
            where TEntity : ITableEntity
        {
            var e = new Tuple<ITableEntity, TableOperation>
                (entity,
                    TableOperation.Replace(entity));
            operations.Enqueue(e);
        }

        public void Execute()
        {
            var count = operations.Count;
            var toExecute = new List<Tuple<ITableEntity, TableOperation>>();
            for (var index = 0; index < count; index++)
            {
                Tuple<ITableEntity, TableOperation> operation;
                operations.TryDequeue(out operation);
                if (operation != null)
                    toExecute.Add(operation);
            }

            toExecute
               .GroupBy(tuple => tuple.Item1.PartitionKey)
               .ToList()
               .ForEach(g =>
               {
                   var opreations = g.ToList();

                   var batch = 0;
                   var operationBatch = GetOperations(opreations, batch);

                   while (operationBatch.Any())
                   {
                       var tableBatchOperation = MakeBatchOperation(operationBatch);

                       ExecuteBatchWithRetries(tableBatchOperation);

                       batch++;
                       operationBatch = GetOperations(opreations, batch);
                   }
               });
        }

        private void ExecuteBatchWithRetries(TableBatchOperation tableBatchOperation)
        {
            var tableRequestOptions = MakeTableRequestOptions();

            var tableReference = MakeTableReference();

            tableReference.ExecuteBatch(tableBatchOperation, tableRequestOptions);
        }

        private static TableRequestOptions MakeTableRequestOptions()
        {
            return new TableRequestOptions
                   {
                       PayloadFormat = TablePayloadFormat.Json,
                       RetryPolicy = new LinearRetry()
                   };
        }

        private static TableBatchOperation MakeBatchOperation(
            List<Tuple<ITableEntity, TableOperation>> operationsToExecute)
        {
            var tableBatchOperation = new TableBatchOperation();
            operationsToExecute.ForEach(tuple => tableBatchOperation.Add(tuple.Item2));
            return tableBatchOperation;
        }

        private static List<Tuple<ITableEntity, TableOperation>> GetOperations(
            IEnumerable<Tuple<ITableEntity, TableOperation>> opreations,
            int batch)
        {
            return opreations
                .Skip(batch * BatchSize)
                .Take(BatchSize)
                .ToList();
        }
    }
}