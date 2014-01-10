using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Brisebois.WindowsAzure.Database
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
            var queryString = Query(model).ToString();

            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                                   "{1}Query as String :{1}{0}{1}",
                                                   queryString,
                                                   Environment.NewLine));

            var query = queryString + GenerateCacheHint();
            var cs = model.Database.Connection.ConnectionString;

            var queryCacheHint = query + cs;

            if (Debugger.IsAttached)
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                                  "{1}Query Cache Key Hint :{1}{0}{1}",
                                                  queryCacheHint,
                                                  Environment.NewLine));

            return MakeCacheKeyHash(queryCacheHint);
        }
    }
}