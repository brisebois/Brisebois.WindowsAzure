namespace Brisebois.WindowsAzure.Database
{
    public class DatabaseSizeRecommendation
    {
        public double CurrentSize { get; set; }
        public int CurrentMaxSize { get; set; }
        public int MaxSize { get; set; }
        public string Edition { get; set; }
    }
}