using System.Data.Entity;
using System.Linq;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class DatabaseScalarDatabaseQuery<TResult, TModel>
        : BaseDatabaseQuery<TModel>, IDatabaseQuery<TResult, TModel> 
        where TModel : DbContext
    {
        protected abstract IQueryable<TResult> Query(TModel model);
      
        public TResult Execute(TModel model)
        {
            return Query(model).SingleOrDefault();
        }
    }
}