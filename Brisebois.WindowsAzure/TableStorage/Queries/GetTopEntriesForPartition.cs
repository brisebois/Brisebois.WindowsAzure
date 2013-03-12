using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage.Queries
{
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

            cacheKey = MakeCacheKeyHash(queryCacheHint);
        }

        public override Task<ICollection<TEntity>> Execute(CloudTable model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            return Task.Run(() =>
                        {
                            var condition = MakePartitionKeyCondition();

                            var tableQuery = new TableQuery<TEntity>();

                            tableQuery = tableQuery.Where(condition).Take(take);

                            return (ICollection<TEntity>)model.ExecuteQuery(tableQuery).ToList();
                        });
        }

        public override string CacheKey
        {
            get { return cacheKey; }
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