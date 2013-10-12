using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Transactions;
using Brisebois.WindowsAzure.Properties;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;

namespace Brisebois.WindowsAzure.Sql
{
    public class Database<TModel>
        : IDatabaseWriteModel<TModel>,
          IDatabaseReadModel<TModel>,
          IDatabaseCachedReadModel<TModel>

        where TModel : IDisposable, new()
    {
        private readonly Func<TModel> factory;
        private TModel model;
        private bool disposeOnCompleted = true;

        public Database(Func<TModel> modelFactory)
        {
            if(modelFactory == null)
                throw new ArgumentNullException("modelFactory");

            factory = modelFactory;
            model = modelFactory();
        }

        public Database<TModel> PreserveModel()
        {
            disposeOnCompleted = false;
            return this;
        } 

        public Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query)
        {
            return QueryAsync(query, RetryParams.Default);
        }

        public async Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query, RetryParams retryParams)
        {
            if (model == null)
                throw new NullReferenceException("The Model has been disposed.");

            var policy = MakeRetryPolicy(retryParams);

            return await policy.ExecuteAsync(() => Task.Run(() =>
                {
                    var result = query.Execute(model);
                    if (disposeOnCompleted)
                    {
                        model.Dispose();
                        model = default(TModel);
                    }
                    return result;
                })).ConfigureAwait(false);
        }

        public string CacheHint<TResult>(IDatabaseQuery<TResult, TModel> query)
        {
            if(query ==null)
                throw new ArgumentNullException("query");
            return query.CacheHint(model);
        }

        public Task<TResult> QueryAsync<TResult>(Func<TModel, TResult> query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            return QueryAsync(query, RetryParams.Default);
        }

        public async Task<TResult> QueryAsync<TResult>(Func<TModel, TResult> query,
                                                       RetryParams retryParams)
        {
            if (model == null)
                throw new NullReferenceException("The Model has been disposed.");

            var policy = MakeRetryPolicy(retryParams);

            return await policy.ExecuteAsync(() => Task.Run(() =>
                {
                    var result = query(model);
                    if (disposeOnCompleted)
                    {
                        model.Dispose();
                        model = default(TModel);
                    }
                    return result;
                })).ConfigureAwait(false);
        }

        public Task DoAsync(Action<TModel> action)
        {
            return DoAsync(action, RetryParams.Default);
        }

        public Task DoAsync(Action<TModel> action, RetryParams retryParams)
        {
            var policy = MakeRetryPolicy(retryParams);

            return policy.ExecuteAsync(() => ExecuteWithTransaction(action));
        }

        public Task DoWithoutTransactionAsync(Action<TModel> action)
        {
            return DoWithoutTransactionAsync(action, RetryParams.Default);
        }

        public Task DoWithoutTransactionAsync(Action<TModel> action, RetryParams retryParams)
        {
            var policy = MakeRetryPolicy(retryParams);

            return policy.ExecuteAsync(() =>
                {
                    using (var shortLivedModel = factory())
                    {
                        action(shortLivedModel);
                    }

                    return Task.FromResult(Resources.Database_Action_Completed);
                });
        }

        public static Database<TModel> Model(Func<TModel> modelFactory)
        {
            return new Database<TModel>(modelFactory);
        }

        /// <summary>
        ///     Default cache of 1 minute
        /// </summary>
        public DatabaseCache<TModel> WithCache()
        {
            return WithCache(null);
        }

        public DatabaseCache<TModel> WithCache(CacheItemPolicy policy)
        {
            if (policy == null)
                policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTime
                            .UtcNow
                            .Add(TimeSpan.FromMinutes(1))
                    };

            return new DatabaseCache<TModel>(this, policy);
        }

        private Task<string> ExecuteWithTransaction(Action<TModel> action)
        {
            var tso = MakeTransactionOptions();

            using (var ts = new TransactionScope(TransactionScopeOption.Required,
                                                 tso))
            {
                using (var shortLivedModel = factory())
                {
                    action(shortLivedModel);
                    ts.Complete();
                }

                return Task.FromResult(Resources.Database_Action_Completed);
            }
        }

        private static TransactionOptions MakeTransactionOptions()
        {
            var tso = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted
                };
            return tso;
        }

        private static RetryPolicy<SqlAzureTransientErrorDetectionStrategy> MakeRetryPolicy(RetryParams retry)
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                retry.MaxRetries,
                TimeSpan.FromMilliseconds(retry.MinBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(retry.MaxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(retry.DeltaBackOffInMilliseconds));
            return policy;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (model != null)
                    model.Dispose();
            }
        }
    }
}