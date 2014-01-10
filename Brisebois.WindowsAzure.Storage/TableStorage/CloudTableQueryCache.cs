using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.Storage.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/03/12/persisting-data-in-windows-azure-table-storage-service/
    /// </summary>
    public class CloudTableQueryCache<TEntity> :
        ICloudTableQuery<TEntity>
        where TEntity : ITableEntity, new()
    {
        private readonly ICloudTableQuery<TEntity> query;
        private readonly CacheItemPolicy cacheItemPolicy;
        private readonly string cacheHint = string.Empty;

        //TODO extract cache dependency in order to make it pluggable
        private readonly static MemoryCache Cache = new MemoryCache("CloudTableQueryCache");

        /// <summary>
        /// Applies a 1 minute cache
        /// </summary>
        public CloudTableQueryCache(ICloudTableQuery<TEntity> query)
            : this(query, null)
        {

        }

        public CloudTableQueryCache(ICloudTableQuery<TEntity> query,
                                    CacheItemPolicy policy)
            : this(query, policy, string.Empty)
        {
        }


        public CloudTableQueryCache(ICloudTableQuery<TEntity> query,
                                    CacheItemPolicy policy,
                                    string cacheHint)
        {
            this.query = query;
            this.cacheHint = cacheHint;

            if (policy == null)
                cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime
                        .UtcNow
                        .Add(TimeSpan.FromMinutes(1))
                };
            else
                cacheItemPolicy = policy;
        }

        public async Task<ICollection<TEntity>> Execute(CloudTable model)
        {
            var cacheKey = GenerateCacheKey(model);

            if (Debugger.IsAttached)
            {
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                              "{0} Cache Key: {1}",
                                              Environment.NewLine,
                                              cacheKey));
            }

            if (Cache.Contains(cacheKey))
            {
                if (Debugger.IsAttached)
                    Trace.WriteLine("Got query result from cache");

                return (ICollection<TEntity>)Cache.Get(cacheKey);
            }

            if (Debugger.IsAttached)
                Trace.WriteLine("Got query result from table storage");

            var entities = await query.Execute(model).ConfigureAwait(false);

            Cache.Add(new CacheItem(cacheKey, entities), cacheItemPolicy);

            return entities;
        }

        public TableRequestOptions MakeTableRequestOptions()
        {
            throw new NotImplementedException();
        }

        public string UniqueIdentifier()
        {
            return query.UniqueIdentifier();
        }

        private string GenerateCacheKey(CloudTable model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var cacheKey = MakeCacheKey(model);
            return MakeCacheKeyHash(cacheKey);
        }

        protected string MakeCacheKeyHash(string queryCacheHint)
        {
            if (string.IsNullOrWhiteSpace(queryCacheHint))
                throw new ArgumentNullException("queryCacheHint");

            using (var unmanaged = new SHA1CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(queryCacheHint);

                var computeHash = unmanaged.ComputeHash(bytes);

                if (computeHash.Length == 0)
                    return string.Empty;

                return Convert.ToBase64String(computeHash);
            }
        }

        private string MakeCacheKey(CloudTable model)
        {
            var cacheKey = query.UniqueIdentifier() + cacheHint + model.Uri;
            return cacheKey;
        }
    }
}