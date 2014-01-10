using System;
using System.Globalization;
using System.Linq;

namespace Brisebois.WindowsAzure.Database.Queries
{
    public class GetDatabaseSizeRecommendation
        : DatabaseScalarQuery<DatabaseSizeRecommendation, EmptyDbContext>
    {
        private readonly string databaseName;

        public GetDatabaseSizeRecommendation(string databaseName)
        {
            this.databaseName = databaseName;
        }

        const string SP = "EXEC [dbo].[GetDatabaseSizeRecommendation] @databasename = {0}";

        protected override IQueryable<DatabaseSizeRecommendation> Query(EmptyDbContext model)
        {
            if(model == null)
                throw new ArgumentNullException("model");

            return model.Database.SqlQuery<DatabaseSizeRecommendation>(SP, databaseName).AsQueryable();
        }

        protected override string GenerateCacheHint()
        {
            return string.Format(CultureInfo.InvariantCulture,SP,databaseName);
        }
    }
}