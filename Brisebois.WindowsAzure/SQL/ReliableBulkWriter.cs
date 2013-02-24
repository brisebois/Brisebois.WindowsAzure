using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure;

namespace Brisebois.WindowsAzure.SQL
{
    /// <summary>
    /// Details: https://alexandrebrisebois.wordpress.com/2013/02/16/using-sqlbulkcopy-to-insert-massive-amounts-of-data-into-windows-azure-sql-database/
    /// </summary>
    public class ReliableBulkWriter
    {
        const int MaxRetry = 5;
        const int DelayMs = 100;

        private readonly string tableName;
        private readonly Dictionary<string, string> tableMap;
        private readonly string connString;

        public ReliableBulkWriter(string tableName,
                                  Dictionary<string, string> tableMap)
        {
            this.tableName = tableName;
            this.tableMap = tableMap;

            // get your connection string
            connString = CloudConfigurationManager
                .GetSetting("ConnectionString");
        }

        public void WriteWithRetries(DataTable datatable)
        {
            TryWrite(datatable);
        }

        private void TryWrite(DataTable datatable)
        {
            var policy = MakeRetryPolicy();
            try
            {
                policy.ExecuteAction(() => Write(datatable));
            }
            catch (Exception ex)
            {

                //TODO: Add logging logic

                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        private void Write(DataTable datatable)
        {
            // connect to SQL
            using (var connection =
                new SqlConnection(connString))
            {
                var bulkCopy = MakeSqlBulkCopy(connection);

                // set the destination table name
                connection.Open();

                using (var dataTableReader = new DataTableReader(datatable))
                {
                    bulkCopy.WriteToServer(dataTableReader);
                }

                connection.Close();
            }
        }

        private RetryPolicy<SqlAzureTransientErrorDetectionStrategy> MakeRetryPolicy()
        {
            var fromMilliseconds = TimeSpan.FromMilliseconds(DelayMs);
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>
                (MaxRetry, fromMilliseconds);
            return policy;
        }

        private SqlBulkCopy MakeSqlBulkCopy(SqlConnection connection)
        {
            var bulkCopy =
                new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    )
                    {
                        DestinationTableName = tableName,
                        EnableStreaming = true
                    };

            tableMap
                .ToList()
                .ForEach(kp =>
                    {
                        bulkCopy
                            .ColumnMappings
                            .Add(kp.Key, kp.Value);
                    });
            return bulkCopy;
        }
    }
}