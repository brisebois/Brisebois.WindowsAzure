using System;
using System.Security.Cryptography;
using System.Text;

namespace Brisebois.WindowsAzure.Sql
{
    public abstract class BaseDatabaseQuery
    {
        protected virtual string GenerateCacheHint()
        {
            return string.Empty;
        }

        protected static string MakeCacheKeyHash(string queryCacheHint)
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
    }
}