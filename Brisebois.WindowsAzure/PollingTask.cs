using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/19/polling-tasks-are-great-building-blocks-for-windows-azure-roles/
    /// </summary>
    public abstract class PollingTask<TWorkItem>
    {
        private Task internalTask;
        private readonly CancellationTokenSource source;
        private int attempts;

        protected PollingTask()
        {
            source = new CancellationTokenSource();
        }

        protected abstract void Report(string message);

        public void Start()
        {
            if (internalTask != null)
                throw new Exception("Task is already running");

            internalTask = Task.Run(() =>
            {
                while (!source.IsCancellationRequested)
                {
                    TryExecuteWorkItems();

                    Report("Heart Beat");
                }
            }, source.Token);
        }

        private void TryExecuteWorkItems()
        {
            try
            {
                var files = GetWork();

                if (files.Any())
                {
                    ResetAttempts();
                    files.AsParallel()
                            .ForAll(ExecuteWork);
                }
                else
                    BackOff();
            }
            catch (Exception ex)
            {
                Report(ex.ToString());
                if (Debugger.IsAttached)
                    Trace.TraceError(ex.ToString());
            }
        }

        private void ExecuteWork(TWorkItem workItem)
        {
            Report(string.Format("Started work on workItem"));
            var w = new Stopwatch();
            w.Start();
            Execute(workItem);
            w.Stop();
            Report(string.Format("Completed work on workItem in {0}",
                                    w.Elapsed.TotalMinutes));
            Completed(workItem);
        }

        protected void BackOff()
        {
            attempts++;

            var seconds = GetTimeoutAsTimeSpan();

            Report(string.Format("Sleep for {0}", seconds));

            Thread.Sleep(seconds);
        }

        private TimeSpan GetTimeoutAsTimeSpan()
        {
            var timeout = DelayCalculator.ExponentialDelay(attempts);

            var seconds = TimeSpan.FromSeconds(timeout);
            return seconds;
        }

        protected abstract void Execute(TWorkItem workItem);
        protected abstract void Completed(TWorkItem workItem);
        protected abstract ICollection<TWorkItem> GetWork();

        public void Cancel()
        {
            source.Cancel();
            internalTask = null;
        }

        public void ResetAttempts()
        {
            attempts = 0;
        }
    }
}
