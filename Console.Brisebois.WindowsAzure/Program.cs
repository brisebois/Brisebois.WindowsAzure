using System;
using System.Diagnostics;
using Brisebois.WindowsAzure.SQL;

namespace Console.Brisebois.WindowsAzure
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var scaler = new SqlDatabaseAutoScaler("myDatabaseName",
                                                    TimeSpan.FromMinutes(5));
            scaler.Start();

            System.Console.ReadLine();
        }
    }
}
