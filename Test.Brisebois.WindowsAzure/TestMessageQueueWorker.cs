using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Brisebois.WindowsAzure.Queues;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Test.Brisebois.WindowsAzure
{
    public class TestMessageQueueWorker : QueueWorker
    {
        public TestMessageQueueWorker(string connectionString,
                                      string queueName,
                                      string poisonQueueName,
                                      int maxAttempts = 10,
                                      int visibilityTimeOutInMinutes = 10)
            : base(connectionString,
                   queueName,
                   poisonQueueName,
                   maxAttempts,
                   visibilityTimeOutInMinutes)
        {
        }

        protected override void Report(string message)
        {
            Console.WriteLine(message);
        }

        protected override void OnExecuting(CloudQueueMessage workItem)
        {
            //Do some work 
            var message = workItem.AsString;
            Trace.WriteLine(message);

            //Used for testing the poison queue
            if (message == "fail")
                throw new Exception(message);

            Task.Delay(TimeSpan.FromSeconds(10))
                .Wait();
        }
    }
}