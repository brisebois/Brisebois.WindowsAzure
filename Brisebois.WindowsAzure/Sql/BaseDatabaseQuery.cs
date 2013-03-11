using System;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Text;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class BaseDatabaseQuery<TModel>
        where TModel : DbContext
    {
        protected abstract string GenerateCacheHint();

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
        
        public string CacheHint(TModel model)
        {
            var query = GenerateCacheHint();
            var cs = model.Database.Connection.ConnectionString;
            return MakeCacheKeyHash(query + cs);
        }
    }
}