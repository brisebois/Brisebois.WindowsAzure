namespace Brisebois.WindowsAzure
{
    public interface IModelCommand<in TModel>
    {
        void Apply(TModel model);
    }
}