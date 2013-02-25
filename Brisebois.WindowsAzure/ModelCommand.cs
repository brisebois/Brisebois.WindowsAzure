namespace Brisebois.WindowsAzure
{
    public interface ModelCommand<in TModel>
    {
        void Apply(TModel model);
    }
}