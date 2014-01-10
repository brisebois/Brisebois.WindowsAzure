namespace Brisebois.WindowsAzure.Database
{
    public interface IDatabaseQuery<out TResult, in TModel>
    {
        string CacheHint(TModel model);
        TResult Execute(TModel model);
    }
}