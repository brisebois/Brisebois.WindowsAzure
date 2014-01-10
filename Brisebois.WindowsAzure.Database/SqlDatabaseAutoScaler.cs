using System;
using System.Text;
using System.Threading.Tasks;
using Brisebois.WindowsAzure.Database.Properties;
using Brisebois.WindowsAzure.Database.Queries;
using Microsoft.WindowsAzure;

namespace Brisebois.WindowsAzure.Database
{
    public class SqlDatabaseAutoScaler : IntervalTask
    {
        private readonly int absoluteMaxSize;
        private readonly string databaseName;

        public SqlDatabaseAutoScaler(string databaseName,
            TimeSpan interval)
            : this(databaseName, interval, 150)
        {
        }

        public SqlDatabaseAutoScaler(string databaseName,
            TimeSpan interval,
            int absoluteMaxSize)
            : base(interval)
        {
            this.databaseName = databaseName;
            this.absoluteMaxSize = absoluteMaxSize;
        }

        protected override async void Execute()
        {
            string bd = CloudConfigurationManager.GetSetting("DatabaseConnectionString");

            var query = new GetDatabaseSizeRecommendation(databaseName);

            DatabaseSizeRecommendation recommendation = await Database<EmptyDbContext>
                .Model(() => new EmptyDbContext(bd))
                .WithCache()
                .QueryAsync(query);

            DatabaseSizeRecommendation databaseSizeRecommendation = recommendation;

            ReportRecommendations(databaseSizeRecommendation);

            if (databaseSizeRecommendation.CurrentMaxSize == databaseSizeRecommendation.MaxSize)
                return;
            if (databaseSizeRecommendation.CurrentMaxSize == absoluteMaxSize)
                return;
            if (databaseSizeRecommendation.MaxSize > absoluteMaxSize)
                return;

            Report(Resources.SqlDatabaseAutoScaler_Applying_Recommendations);

            string m = CloudConfigurationManager.GetSetting("MasterDatabaseConnectionString");

            Task result = Database<EmptyDbContext>.Model(() => new EmptyDbContext(m))
                .DoWithoutTransactionAsync(model => model.Database.ExecuteSqlCommand("ALTER DATABASE ["
                                                                                     + databaseName
                                                                                     + "] MODIFY (EDITION='"
                                                                                     +
                                                                                     databaseSizeRecommendation.Edition
                                                                                     + "', MAXSIZE="
                                                                                     +
                                                                                     databaseSizeRecommendation.MaxSize
                                                                                     + "GB)"));
            result.Wait();
        }

        private void ReportRecommendations(DatabaseSizeRecommendation recommendation)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Current Database Size :{0}", recommendation.CurrentSize);
            sb.AppendLine();
            sb.AppendFormat("Current Database Max Size: {0}", recommendation.CurrentMaxSize);
            sb.AppendLine();
            sb.AppendFormat("Recommended Database Max Size: {0}", recommendation.MaxSize);
            sb.AppendLine();
            sb.AppendFormat("Recommended Database Edition : {0}", recommendation.Edition);

            Report(sb.ToString());
        }

        protected override void Report(string message)
        {
            //Logger.Add("SQLDatabaseAutoScaler", "Event", message);
        }
    }
}