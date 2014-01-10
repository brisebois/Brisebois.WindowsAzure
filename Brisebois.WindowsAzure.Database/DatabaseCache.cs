using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.Database
{
    public class DatabaseCache<TModel> : IDatabaseCachedReadModel<TModel>
    {
        private readonly CacheItemPolicy cacheItemPolicy;
        private readonly IDatabaseCachedReadModel<TModel> model;
        private readonly string cacheHint = string.Empty;

        private readonly static MemoryCache cache = new MemoryCache("DatabaseCache");

        /// <summary>
        /// Applies a 1 minute cache
        /// </summary>
        public DatabaseCache(IDatabaseCachedReadModel<TModel> model)
            : this(model, null)
        {
        }

        public DatabaseCache(IDatabaseCachedReadModel<TModel> model,
                             CacheItemPolicy policy)
            : this(model, policy, string.Empty)
        {
        }


        public DatabaseCache(IDatabaseCachedReadModel<TModel> model,
                             CacheItemPolicy policy,
                             string cacheHint)
        {
            this.model = model;
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

        public Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query)
        {
            return QueryAsync(query, RetryParams.Default);
        }

        public async Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query, RetryParams retryParams)
        {
            var key = model.CacheHint(query) + cacheHint;

            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}Hashed CacheHint :{0}{1}", Environment.NewLine, key));

            if (cache.Contains(key))
                return GetFromCache<TResult>(key);

            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}Query result from server{0}", Environment.NewLine));

            var entities = await model.QueryAsync(query, retryParams)
                                      .ConfigureAwait(false);

            cache.Add(new CacheItem(key, entities), cacheItemPolicy);

            return entities;
        }

        private static TResult GetFromCache<TResult>(string cacheKey)
        {
            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}Query result from cache{0}", Environment.NewLine));

            return (TResult)cache.Get(cacheKey);
        }

        public string CacheHint<TResult>(IDatabaseQuery<TResult, TModel> query)
        {
            return model.CacheHint(query);
        }
    }
}