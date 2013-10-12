using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage.Queries
{
    public class GetWindowsAzureDiagnostics<TEntity> :
        CloudTableQuery<TEntity>
        where TEntity : ITableEntity, new()
    {
        private readonly string tableStartPartition;

        private readonly string cacheKey;
        private readonly string tableEndPartition;

        public GetWindowsAzureDiagnostics(DateTime start, DateTime end)
        {
            tableStartPartition = "0" + start.ToUniversalTime().Ticks;
            tableEndPartition = "0" + end.ToUniversalTime().Ticks;

            var queryCacheHint = "GetWindowsAzureDiagnostics"
                                    + tableStartPartition
                                    + tableEndPartition;

            cacheKey = queryCacheHint;
        }

        public override Task<ICollection<TEntity>> Execute(CloudTable model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            return Task.Run(() =>
            {
                var condition = MakePartitionKeyCondition();

                var tableQuery = new TableQuery<TEntity>();

                tableQuery = tableQuery.Where(condition);

                return (ICollection<TEntity>)model.ExecuteQuery(tableQuery).ToList();
            });
        }

        public override string GenerateCacheKey(CloudTable model)
        {
            return cacheKey;
        }

        private string MakePartitionKeyCondition()
        {
            var startTicks = tableStartPartition.ToUpperInvariant();
            var partitionStarts = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.GreaterThanOrEqual,
                startTicks);

            var endTicks = tableEndPartition.ToUpperInvariant();
            var partitionEnds = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.LessThanOrEqual,
                endTicks);

            return TableQuery.CombineFilters(partitionStarts, TableOperators.And, partitionEnds);
        }
    }
}