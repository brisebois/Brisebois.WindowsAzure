using System.Data.Entity;
using System.Linq;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class DatabaseScalarQuery<TResult, TModel>
        : BaseDatabaseQuery, IDatabaseQuery<TResult, TModel> 
        where TModel : DbContext
    {
        protected abstract IQueryable<TResult> Query(TModel model);
      
        public TResult Execute(TModel model)
        {
            return Query(model).SingleOrDefault();
        }

        public string CacheHint(TModel model)
        {
            var query = Query(model) + GenerateCacheHint();
            var cs = model.Database.Connection.ConnectionString;
            return MakeCacheKeyHash(query + cs);
        }
    }
}