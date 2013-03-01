namespace Brisebois.WindowsAzure
{
    public interface WorkerProcess
    {
        void Start();
        void Cancel();
    }
}