using System;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure.Database
{
    public interface IDatabaseWriteModel<out TModel>
    {
        Task DoAsync(Action<TModel> action);
        Task DoAsync(Action<TModel> action, RetryParams retryParams);
        Task DoWithoutTransactionAsync(Action<TModel> action);
        Task DoWithoutTransactionAsync(Action<TModel> action, RetryParams retryParams);
    }
}