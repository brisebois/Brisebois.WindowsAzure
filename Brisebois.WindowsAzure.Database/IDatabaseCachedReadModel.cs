using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.Database
{
    public interface IDatabaseCachedReadModel<out TModel>
    {
        Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query);

        Task<TResult> QueryAsync<TResult>(IDatabaseQuery<TResult, TModel> query,
            RetryParams retryParams);

        string CacheHint<TResult>(IDatabaseQuery<TResult, TModel> query);
    }
}