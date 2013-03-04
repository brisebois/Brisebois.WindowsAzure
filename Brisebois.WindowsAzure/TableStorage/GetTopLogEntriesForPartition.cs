using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class GetTopLogEntriesForPartition<TEntity> : CloudTableQuery<TEntity> where TEntity : ITableEntity, new()
    {
        private readonly string tablePartition;
        private readonly int take;

        public GetTopLogEntriesForPartition(string partition)
            :this(partition,100)
        {

        }

        public GetTopLogEntriesForPartition(string partition, int take)
        {
            if(partition == null)
                throw new ArgumentNullException("partition");

            tablePartition = partition.ToUpperInvariant();
            this.take = take;
        }

        public override ICollection<TEntity> Execute(CloudTable model)
        {
            if(model == null)
                throw new ArgumentNullException("model");

            var value = string.Format(CultureInfo.InvariantCulture, "{0}", tablePartition).ToUpperInvariant();
            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, value);

            var tableQuery = new TableQuery<TEntity>();
            tableQuery.Where(condition).Take(take);

            var newsItems = model.ExecuteQuery(tableQuery).ToList();

            return newsItems;
        }
    }
}