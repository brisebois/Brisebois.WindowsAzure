using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/03/12/persisting-data-in-windows-azure-table-storage-service/
    /// </summary>
    public abstract class CloudTableQuery<TEntity>
        : IModelQuery<Task<ICollection<TEntity>>, CloudTable>, ICloudTableQuery<TEntity> where TEntity : ITableEntity, new()
    {
        public virtual Task<ICollection<TEntity>> Execute(CloudTable model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            return Task.Run(() =>
            {
                var tableQuery = MakeTableQuery();

                return (ICollection<TEntity>)model.ExecuteQuery(tableQuery, MakeTableRequestOptions()).ToList();
            });
        }

        public virtual TableRequestOptions MakeTableRequestOptions()
        {
            return new TableRequestOptions
                   {
                       PayloadFormat = TablePayloadFormat.JsonNoMetadata,
                       RetryPolicy = new LinearRetry(TimeSpan.FromMilliseconds(1), 100),
                   };
        }

        protected virtual TableQuery<TEntity> MakeTableQuery()
        {
            return new TableQuery<TEntity>();
        }

        public abstract string UniqueIdentifier();
    }
}