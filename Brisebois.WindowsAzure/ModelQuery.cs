namespace Brisebois.WindowsAzure
{
    public interface ModelQuery<out TResult, in TModel>
    {
        TResult Execute(TModel model);
    }
}