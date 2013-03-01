using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Brisebois.WindowsAzure
{
    public abstract class IntervalTask : WorkerProcess
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
                throw new Exception("Task is already running");

            internalTask = Task.Run(() =>
                {
                    while (!source.IsCancellationRequested)
                    {
                        TryExecute();

                        Report("Heart Beat");
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
            catch (Exception ex)
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
    }
}