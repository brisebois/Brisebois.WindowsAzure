namespace Brisebois.WindowsAzure
{
    public interface IWorkerProcess
    {
        void Start();
        void Cancel();
    }
}