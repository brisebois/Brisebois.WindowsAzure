using System;
using System.Linq;
using System.Text;
using Brisebois.WindowsAzure.Properties;
using Brisebois.WindowsAzure.TableStorage;
using Microsoft.WindowsAzure;

namespace Brisebois.WindowsAzure.Sql
{
    public class SqlDatabaseAutoScaler : IntervalTask
    {
        private readonly string databaseName;
        private readonly int absoluteMaxSize;

        public SqlDatabaseAutoScaler(string databaseName,
                                     TimeSpan interval)
            : this(databaseName,interval,150)
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

        protected override void Execute()
        {
            var bd = CloudConfigurationManager.GetSetting("DatabaseConnectionString");
            const string sp = "EXEC [dbo].[GetDatabaseSizeRecommendation] @databasename = {0}";

            var recommendation = ReliableModel.Query(model => model.Database.SqlQuery<DatabaseSizeRecommendation>(
                                        sp,
                                        databaseName)
                                        .FirstOrDefault(), () => new EmptyDbContext(bd));

            ReportRecommendations(recommendation);

            if (recommendation.CurrentMaxSize == recommendation.MaxSize)
                return;
            if (recommendation.CurrentMaxSize == absoluteMaxSize)
                return;
            if (recommendation.MaxSize > absoluteMaxSize)
                return;

            Report(Resources.SqlDatabaseAutoScaler_Applying_Recommendations);

            var m = CloudConfigurationManager.GetSetting("MasterDatabaseConnectionString");

            ReliableModel.DoWithoutTransaction(model => model.Database.ExecuteSqlCommand("ALTER DATABASE ["
                                                                                         + databaseName
                                                                                         + "] MODIFY (EDITION='"
                                                                                         + recommendation.Edition
                                                                                         + "', MAXSIZE="
                                                                                         + recommendation.MaxSize
                                                                                         + "GB)"), () => new EmptyDbContext(m));
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
            Logger.Add("SQLDatabaseAutoScaler", "Event", message);
        }
    }
}