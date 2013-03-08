using System.Runtime.Caching;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public static class CloudTableQueryExtensions
    {
        /// <summary>
        /// Applies a 1 minute cache
        /// </summary>
        public static CloudTableQuery<TEntity> WithCache<TEntity>(this CloudTableQuery<TEntity> query)
            where TEntity : ITableEntity, new()
        {
            return new CloudTableQueryCache<TEntity>(query);
        }

        public static CloudTableQuery<TEntity> WithCache<TEntity>(this CloudTableQuery<TEntity> query, 
                                                                  CacheItemPolicy policy)
            where TEntity : ITableEntity, new()
        {
            return new CloudTableQueryCache<TEntity>(query, policy);
        }

        public static CloudTableQuery<TEntity> WithCache<TEntity>(this CloudTableQuery<TEntity> query,
                                                                  CacheItemPolicy policy,
                                                                  string cacheHint)
            where TEntity : ITableEntity, new()
        {
            return new CloudTableQueryCache<TEntity>(query, policy, cacheHint);
        }
    }
}