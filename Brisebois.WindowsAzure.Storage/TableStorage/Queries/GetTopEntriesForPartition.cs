using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage.Queries
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/03/12/persisting-data-in-windows-azure-table-storage-service/
    /// </summary>
    public class GetTopEntriesForPartition<TEntity> :
        CloudTableQuery<TEntity>
        where TEntity : ITableEntity, new()
    {
        private readonly string tablePartition;
        private readonly int take;
        private readonly string cacheKey;

        public GetTopEntriesForPartition(string partition)
            : this(partition, 100)
        {
        }

        public GetTopEntriesForPartition(string partition, int take)
        {
            if (partition == null)
                throw new ArgumentNullException("partition");

            tablePartition = partition;
            this.take = take;

            var queryCacheHint = "GetTopEntriesForPartition"
                                 + tablePartition
                                 + take;

            cacheKey = queryCacheHint;
        }
        
        protected override TableQuery<TEntity> MakeTableQuery()
        {
            var condition = MakePartitionKeyCondition();

            var tableQuery = new TableQuery<TEntity>();

            tableQuery = tableQuery.Where(condition).Take(take);
            return tableQuery;
        }

        public override string UniqueIdentifier()
        {
            return cacheKey;
        }

        private string MakePartitionKeyCondition()
        {
            var value = tablePartition.ToUpperInvariant();
            return TableQuery.GenerateFilterCondition("PartitionKey",
                                                 QueryComparisons.Equal,
                                                 value);
        }
    }
}