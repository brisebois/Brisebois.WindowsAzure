using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class DatabaseScalarQuery<TResult, TModel>
        : IDatabaseQuery<TResult, TModel> 
        where TModel : DbContext
    {
        protected abstract IQueryable<TResult> Query(TModel model);

        protected abstract string GenerateCacheHint();

        public string CacheHint(TModel model)
        {
            var query = GenerateCacheHint();
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
        
        public TResult Execute(TModel model)
        {
            return Query(model).SingleOrDefault();
        }
    }
}