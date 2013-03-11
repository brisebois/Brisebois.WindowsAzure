using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class DatabaseQuery<TResult, TModel>
        : BaseDatabaseQuery, IDatabaseQuery<ICollection<TResult>, TModel>
        where TModel : DbContext
    {
        protected abstract IQueryable<TResult> Query(TModel model);

        public ICollection<TResult> Execute(TModel model)
        {
            return Query(model).ToList();
        }

        public string CacheHint(TModel model)
        {
            var query = Query(model) + GenerateCacheHint();
            var cs = model.Database.Connection.ConnectionString;
            return MakeCacheKeyHash(query + cs);
        }
    }
}