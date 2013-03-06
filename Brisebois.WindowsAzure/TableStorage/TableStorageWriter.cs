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
    public class TableStorageWriter
    {
        private readonly CloudStorageAccount storageAccount;
        
        private readonly ConcurrentQueue<Tuple<ITableEntity,TableOperation>> operations;
        private readonly string tableName;

        private const int BatchSize = 100;

        public TableStorageWriter(string tableName)
        {
            this.tableName = tableName;

            var cs = CloudConfigurationManager.GetSetting("StorageConnectionString");
            
            storageAccount = CloudStorageAccount.Parse(cs);
            
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference(tableName);
            tableReference.CreateIfNotExists();

            operations = new ConcurrentQueue<Tuple<ITableEntity, TableOperation>>();
        }

        public decimal OutstandingOperations
        {
            get { return operations.Count; }
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.Insert(entity)));
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.Delete(entity)));
        }

        public void InsertOrMerge<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.InsertOrMerge(entity)));
        }

        public void InsertOrReplace<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.InsertOrReplace(entity)));
        }

        public void Merge<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.Merge(entity)));
        }

        public void Replace<TEntity>(TEntity entity) where TEntity : ITableEntity
        {
            operations.Enqueue(new Tuple<ITableEntity, TableOperation>(entity,TableOperation.Replace(entity)));
        }

        public void Execute()
        {
            var count = operations.Count;
            var operationsToExecute = new List<Tuple<ITableEntity, TableOperation>>();
            for (var index = 0; index < count; index++)
            {
                Tuple<ITableEntity, TableOperation> operation;
                operations.TryDequeue(out operation);
                if(operation!= null)
                    operationsToExecute.Add(operation);
            }

           operationsToExecute
                .GroupBy(tuple => tuple.Item1.PartitionKey)
                .ToList()
                .ForEach(g => {

                        var opreations = g.ToList();

                        var operationBatch = opreations
                            .Take(BatchSize)
                            .ToList();

                        var batch = 0;
                        while (operationsToExecute.Any())
                        {
                            var tableBatchOperation = MakeBatchOperation(operationBatch);

                            ExecuteBatchWithRetries(tableBatchOperation);

                            batch++;
                            operationsToExecute = GetOperations(opreations, batch);
                        }
                    });
        }

        private void ExecuteBatchWithRetries(TableBatchOperation tableBatchOperation)
        {
            var tableRequestOptions = MakeTableRequestOptions();
            
            var tableClient = storageAccount.CreateCloudTableClient();
            var tableReference = tableClient.GetTableReference(tableName);    
            tableReference.ExecuteBatch(tableBatchOperation, tableRequestOptions);
        }

        private static TableRequestOptions MakeTableRequestOptions()
        {
            return new TableRequestOptions
                {
                    RetryPolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(2), 100)
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