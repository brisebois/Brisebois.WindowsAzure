using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure.Sql
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2012/10/15/using-the-exponential-back-off-transient-error-detection-strategy/
    /// </summary>
    [Obsolete("This class will be removed in the next version")]
    public static class ReliableModel
    {
        public static void Do<TModel>(Action<TModel> action)
            where TModel : IDisposable, new()
        {
            Do(action,
                10,
                20,
                8000,
                20);
        }

        public static void Do<TModel>(
            Action<TModel> action,
            int maxRetries,
            int minBackOffDelayInMilliseconds,
            int maxBackOffDelayInMilliseconds,
            int deltaBackOffInMilliseconds)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackOffInMilliseconds));

            policy.ExecuteAction(() =>
                {
                    var tso = new TransactionOptions
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted
                        };

                    using (var ts = new TransactionScope(TransactionScopeOption.Required,
                                                         tso))
                    {
                        using (var model = new TModel())
                        {
                            action(model);
                        }
                        ts.Complete();
                    }
                });
        }

        public static void Do<TModel>(
            Action<TModel> action,
            Func<TModel> createModel)

            where TModel : IDisposable, new()
        {
            Do(action, createModel, 10, 20, 8000, 20);
        }

        public static void Do<TModel>(
            Action<TModel> action,
            Func<TModel> createModel,
            int maxRetries,
            int minBackOffDelayInMilliseconds,
            int maxBackOffDelayInMilliseconds,
            int deltaBackOffInMilliseconds)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackOffInMilliseconds));

            policy.ExecuteAction(() =>
                {
                    var tso = new TransactionOptions
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted,
                            Timeout = TimeSpan.FromHours(24)
                        };
                    using (var ts = new TransactionScope(TransactionScopeOption.Required,
                                                         tso))
                    {
                        using (var model = createModel())
                        {
                            action(model);
                        }
                        ts.Complete();
                    }
                });
        }

        public static void DoWithoutTransaction<TModel>(Action<TModel> action,
                                                        Func<TModel> createModel)
            where TModel : IDisposable, new()
        {
            DoWithoutTransaction(action,
                createModel,
                10,
                20,
                8000,
                20);
        }

        public static void DoWithoutTransaction<TModel>(
            Action<TModel> action,
            Func<TModel> createModel,
            int maxRetries,
            int minBackOffDelayInMilliseconds,
            int maxBackOffDelayInMilliseconds,
            int deltaBackOffInMilliseconds)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackOffInMilliseconds));

            policy.ExecuteAction(() =>
                {
                    using (var model = createModel())
                    {
                        action(model);
                    }
                });
        }

        public static Task<TResult> QueryAsync<TModel, TResult>(
            Func<TModel, TResult> query,
            Func<TModel> createModel)

            where TModel : IDisposable, new()
        {
            return QueryAsync(query,
                              createModel,
                              10,
                              20,
                              8000,
                              20);
        }

        public static async Task<TResult> QueryAsync<TModel, TResult>(
            Func<TModel, TResult> query,
            Func<TModel> createModel,
            int maxRetries,
            int minBackOffDelayInMilliseconds,
            int maxBackOffDelayInMilliseconds,
            int deltaBackOffInMilliseconds)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackOffInMilliseconds));

            return await policy.ExecuteAsync(() => Task.Factory.StartNew(() =>
                {
                    using (var model = createModel())
                    {
                        return query(model);
                    }
                })).ConfigureAwait(false); ;
        }

        public static TResult Query<TModel, TResult>(
            Func<TModel, TResult> queryFunc,
            Func<TModel> createModel)

            where TModel : IDisposable, new()
        {
            return Query(queryFunc,
                         createModel,
                         10,
                         20,
                         8000,
                         20);
        }

        public static TResult Query<TModel, TResult>(
            Func<TModel, TResult> queryFunc,
            Func<TModel> createModel,
            int maxRetries,
            int minBackOffDelayInMilliseconds,
            int maxBackOffDelayInMilliseconds,
            int deltaBackOffInMilliseconds)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackOffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackOffInMilliseconds));

            return policy.ExecuteAction(() =>
                {
                    using (var model = createModel())
                    {
                        return queryFunc(model);
                    }
                });
        }
    }
}