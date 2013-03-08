using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class DatabaseQuery<TResult, TModel>
        : IDatabaseQuery<ICollection<TResult>, TModel> 
        where TModel : DbContext
    {
        protected abstract IQueryable<TResult> Query(TModel model);

        public string CacheHint(TModel model)
        {
            var query = Query(model);
            var cs = model.Database.Connection.ConnectionString;
            return MakeCacheKeyHash(query + cs);
        }

        protected string MakeCacheKeyHash(string queryCacheHint)
        {
            if (string.IsNullOrWhiteSpace(queryCacheHint))
                throw new ArgumentNullException("queryCacheHint");

            using (var unmanaged = new SHA1CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(queryCacheHint);

                var computeHash = unmanaged.ComputeHash(bytes);

                if (computeHash.Length == 0)
                    return string.Empty;

                return Convert.ToBase64String(computeHash);
            }
        }

        public ICollection<TResult> Execute(TModel model)
        {
            return Query(model).ToList();
        }
    }
}