using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage.Queries
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

        private static IEnumerable<TEntity> ExecuteQuery(CloudTable model, TableQuery<TEntity> tableQuery)
        {
            return model.ExecuteQuery(tableQuery);
        }

        protected override TableQuery<TEntity> MakeTableQuery()
        {
            var condition = MakePartitionKeyCondition();

            var tableQuery = new TableQuery<TEntity>();

            tableQuery = tableQuery.Where(condition);
            return tableQuery;
        }

        public override string UniqueIdentifier()
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