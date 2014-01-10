using System.Data.Entity;

namespace Brisebois.WindowsAzure.Database
{
    public class EmptyDbContext : DbContext
    {
        public EmptyDbContext()
        {
            System.Data.Entity.Database.SetInitializer<EmptyDbContext>(null);
        }
        public EmptyDbContext(string connectionString)
            : base(connectionString)
        {
            System.Data.Entity.Database.SetInitializer<EmptyDbContext>(null);
        }
    }
}