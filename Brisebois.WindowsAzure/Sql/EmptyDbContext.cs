using System.Data.Entity;

namespace Brisebois.WindowsAzure.Sql
{
    public class EmptyDbContext : DbContext
    {
        public EmptyDbContext()
        {
            Database.SetInitializer<EmptyDbContext>(null);
        }

        public EmptyDbContext(string connectionString)
            : base(connectionString)
        {
            Database.SetInitializer<EmptyDbContext>(null);
        }
    }
}