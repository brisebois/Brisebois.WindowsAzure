using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Brisebois.WindowsAzure.Properties;

namespace Brisebois.WindowsAzure
{
    public abstract class IntervalTask : IWorkerProcess, IDisposable
    {
        private Task internalTask;
        private readonly CancellationTokenSource source;
        private readonly TimeSpan interval;

        protected IntervalTask(TimeSpan interval)
        {
            this.interval = interval;
            source = new CancellationTokenSource();
        }

        public void Start()
        {
            if (internalTask != null)
                throw new IntervalTaskException("Task is already running");

            internalTask = Task.Run(() =>
                {
                    while (!source.IsCancellationRequested)
                    {
                        TryExecute();

                        Report(Resources.Heart_Beat);
                    }
                }, source.Token);
        }

        private void TryExecute()
        {
            try
            {
                Task.Delay(interval)
                    .ContinueWith(_ => Execute())
                    .Wait();
            }
            catch (AggregateException ex)
            {
                Report(ex.ToString());
                if (Debugger.IsAttached)
                    Trace.TraceError(ex.ToString());
            }
        }

        protected abstract void Execute();
        protected abstract void Report(string message);

        public void Cancel()
        {
            source.Cancel();
            internalTask = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               internalTask.Dispose();
               source.Dispose();
            }
            internalTask = null;
        }
    }
}