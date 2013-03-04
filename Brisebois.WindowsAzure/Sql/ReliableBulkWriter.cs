using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Brisebois.WindowsAzure.TableStorage;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.WindowsAzure;

namespace Brisebois.WindowsAzure.Sql
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/16/using-sqlbulkcopy-to-insert-massive-amounts-of-data-into-windows-azure-sql-database/
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

        public void WriteWithRetries(DataTable dataTable)
        {
            TryWrite(dataTable);
        }

        private void TryWrite(DataTable dataTable)
        {
            var policy = MakeRetryPolicy();
            try
            {
                policy.ExecuteAction(() => Write(dataTable));
            }
            catch (Exception ex)
            {
                Logger.Add("ReliableBulkWriter", "Exception", ex.ToString());
                throw;
            }
        }

        private void Write(DataTable dataTable)
        {
            // connect to SQL
            using (var connection = new SqlConnection(connString))
            {
                // set the destination table name
                connection.Open();

                using (var bulkCopy = MakeSqlBulkCopy(connection))
                {               
                    using (var dataTableReader = new DataTableReader(dataTable))
                    {
                        bulkCopy.WriteToServer(dataTableReader);
                    }                    
                }
            }
        }

        private static RetryPolicy<SqlAzureTransientErrorDetectionStrategy> MakeRetryPolicy()
        {
            var fromMilliseconds = TimeSpan.FromMilliseconds(DelayMs);
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>
                (MaxRetry, fromMilliseconds);
            return policy;
        }

        private SqlBulkCopy MakeSqlBulkCopy(SqlConnection connection)
        {
            SqlBulkCopy bulkCopy;

            SqlBulkCopy tempBulkCopy = null;

            try
            {
                tempBulkCopy = new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                tempBulkCopy.EnableStreaming = true;
                tempBulkCopy.DestinationTableName = tableName;

                tableMap
                .ToList()
                .ForEach(kp => tempBulkCopy
                                   .ColumnMappings
                                   .Add(kp.Key, kp.Value));

                bulkCopy = tempBulkCopy;
            
                tempBulkCopy = null;
            }
            finally 
            {
                if(tempBulkCopy != null)
                    tempBulkCopy.Close();
            }
            
            return bulkCopy;
        }
    }
}