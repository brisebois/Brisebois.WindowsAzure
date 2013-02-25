using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
using Microsoft.Practices.TransientFaultHandling;

namespace Brisebois.WindowsAzure.SQL
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2012/10/15/using-the-exponential-back-off-transient-error-detection-strategy/
    /// </summary>
    public class ReliableModel
    {
        public static void Do<TModel>(
            Action<TModel> action,
            int maxRetries = 4,
            int minBackoffDelayInMilliseconds = 2000,
            int maxBackoffDelayInMilliseconds = 8000,
            int deltaBackoffInMilliseconds = 2000)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackoffInMilliseconds));

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
            Func<TModel> createModel,
            int maxRetries = 4,
            int minBackoffDelayInMilliseconds = 2000,
            int maxBackoffDelayInMilliseconds = 8000,
            int deltaBackoffInMilliseconds = 2000)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackoffInMilliseconds));

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

        public static void DoWithoutTransaction<TModel>(
            Action<TModel> action,
            Func<TModel> createModel,
            int maxRetries = 4,
            int minBackoffDelayInMilliseconds = 2000,
            int maxBackoffDelayInMilliseconds = 8000,
            int deltaBackoffInMilliseconds = 2000)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackoffInMilliseconds));

            policy.ExecuteAction(() =>
                {
                    using (var model = createModel())
                    {
                        action(model);
                    }
                });
        }

        public static async Task<TResult> QueryAsync<TModel, TResult>(
            Func<TModel, TResult> query,
            Func<TModel> createModel,
            int maxRetries = 4,
            int minBackoffDelayInMilliseconds = 2000,
            int maxBackoffDelayInMilliseconds = 8000,
            int deltaBackoffInMilliseconds = 2000)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackoffInMilliseconds));

            return await policy.ExecuteAsync(() => Task.Factory.StartNew(() =>
                {
                    using (var model = createModel())
                    {
                        return query(model);
                    }
                })).ConfigureAwait(false); ;
        }
        
        public static TResult Query<TModel, TResult>(
            Func<TModel, TResult> query,
            Func<TModel> createModel,
            int maxRetries = 4,
            int minBackoffDelayInMilliseconds = 2000,
            int maxBackoffDelayInMilliseconds = 8000,
            int deltaBackoffInMilliseconds = 2000)

            where TModel : IDisposable, new()
        {
            var policy = new RetryPolicy<SqlAzureTransientErrorDetectionStrategy>(
                maxRetries,
                TimeSpan.FromMilliseconds(minBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(maxBackoffDelayInMilliseconds),
                TimeSpan.FromMilliseconds(deltaBackoffInMilliseconds));

            return policy.ExecuteAction(() =>
                {
                    using (var model = createModel())
                    {
                        return query(model);
                    }
                });
        }
    }
}