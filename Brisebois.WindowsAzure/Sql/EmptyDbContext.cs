using System.Data.Entity;

namespace Brisebois.WindowsAzure.Sql
{
    public class EmptyDbContext : DbContext
    {
        public EmptyDbContext()
        {

        }

        public EmptyDbContext(string connectionString)
            : base(connectionString)
        {

        }
    }
}