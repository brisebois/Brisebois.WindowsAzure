using Microsoft.WindowsAzure.Storage.Queue;

namespace Brisebois.WindowsAzure.Queues
{
public class ShardedQueueMessage
{
    internal CloudQueue OriginQueue { get; set; }
    public CloudQueueMessage Message { get; set; }
}
}