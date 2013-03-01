using System.Data.Entity;

namespace Brisebois.WindowsAzure.SQL
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