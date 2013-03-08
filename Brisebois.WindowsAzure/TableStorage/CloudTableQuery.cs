using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public abstract class CloudTableQuery<TEntity> : IModelQuery<ICollection<TEntity>, CloudTable>
    {
        public abstract ICollection<TEntity> Execute(CloudTable model);

        public abstract string CacheKey { get; }

        protected string MakeCacheKeyHash(string queryCacheHint)
        {
            if(string.IsNullOrWhiteSpace(queryCacheHint))
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
    }
}