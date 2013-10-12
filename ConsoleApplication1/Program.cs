using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brisebois.WindowsAzure.TableStorage;
using Brisebois.WindowsAzure.TableStorage.Queries;
using Microsoft.WindowsAzure.Storage.Table;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Test();
            t.GetDiagnostics().ContinueWith(task =>
            {
                var r = task.Result;

                foreach (var dynamicEntity in r)
                {
                    Console.WriteLine(dynamicEntity["Description"].StringValue);
                }
            });

            Console.ReadLine();
        }
    }

    public class Test
    {
        public Test()
        {
            GetDiagnostics();
        }

public async Task<ICollection<DynamicTableEntity>> GetDiagnostics()
{
    TimeSpan oneHour = TimeSpan.FromHours(1);
    DateTime start = DateTime.UtcNow.Subtract(oneHour);

    var query = new GetWindowsAzureDiagnostics<DynamicTableEntity>(start, DateTime.UtcNow);

    return await TableStorageReader.Table("WADLogsTable").Execute(query);
}
    }
}
