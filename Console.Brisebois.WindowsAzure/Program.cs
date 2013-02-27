using System;
using Brisebois.WindowsAzure.Blobs;

namespace Console.Brisebois.WindowsAzure
{
class Program
{
    static void Main(string[] args)
    {
        var remover = new BlobSnapshotCleaner("StorageConnectionString", 
                                                "documents",
                                                TimeSpan.MinValue);
        remover.Start();

        System.Console.ReadLine();
    }
}
}
