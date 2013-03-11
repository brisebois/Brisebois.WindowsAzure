using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class TableStorageReader
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable tableReference;

        public TableStorageReader(string tableName)
        {
            var cs = CloudConfigurationManager.GetSetting("StorageConnectionString");
            storageAccount = CloudStorageAccount.Parse(cs);
            tableClient = storageAccount.CreateCloudTableClient();
            tableReference = tableClient.GetTableReference(tableName);
            tableReference.CreateIfNotExists();
        }

        public ICollection<TEntity> Execute<TEntity>(CloudTableQuery<TEntity> query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            return query.Execute(tableReference);
        }
    }
}