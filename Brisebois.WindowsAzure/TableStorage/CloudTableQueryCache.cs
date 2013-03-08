using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class CloudTableQueryCache<TEntity> :
        CloudTableQuery<TEntity>
        where TEntity : ITableEntity, new()
    {
        private readonly CloudTableQuery<TEntity> query;
        private readonly CacheItemPolicy cacheItemPolicy;
        private readonly string cacheHint = string.Empty;

        private readonly static MemoryCache cache = new MemoryCache("CloudTableQueryCache");

        /// <summary>
        /// Applies a 1 minute cache
        /// </summary>
        public CloudTableQueryCache(CloudTableQuery<TEntity> query)
            : this(query, null)
        {
        
        }

        public CloudTableQueryCache(CloudTableQuery<TEntity> query,
                                    CacheItemPolicy policy)
            : this(query, policy, string.Empty)
        {
        }


        public CloudTableQueryCache(CloudTableQuery<TEntity> query,
                                    CacheItemPolicy policy,
                                    string cacheHint)
        {
            this.query = query;
            this.cacheHint = cacheHint;

            if(policy== null)
                cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime
                        .UtcNow
                        .Add(TimeSpan.FromMinutes(1))
                };
            else
                cacheItemPolicy = policy;
        }

        public override ICollection<TEntity> Execute(CloudTable model)
        {
            var cacheKey = query.CacheKey + cacheHint;

            if (cache.Contains(cacheKey))
                return (ICollection<TEntity>)cache.Get(cacheKey);

            var entities = query.Execute(model);

            cache.Add(new CacheItem(cacheKey, entities), cacheItemPolicy);

            return entities;
        }

        public override string CacheKey
        {
            get
            {
                return GetHashCode().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}