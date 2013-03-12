using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class CloudTableQueryCache<TEntity> :
        CloudTableQuery<TEntity>
        where TEntity : ITableEntity
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

        public override async Task<ICollection<TEntity>> Execute(CloudTable model)
        {
            var cacheKey = query.CacheKey + cacheHint;

            if (Debugger.IsAttached)
            {
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                              "{0} Cache Key: {1}", 
                                              Environment.NewLine, 
                                              cacheKey));
            }

            if (cache.Contains(cacheKey))
            {
                if (Debugger.IsAttached)
                    Trace.WriteLine("Got query result from cache");

                return (ICollection<TEntity>)cache.Get(cacheKey);
            }

            if (Debugger.IsAttached)
                Trace.WriteLine("Got query result from table storage");

            var entities = await query.Execute(model)
                                      .ConfigureAwait(false);

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