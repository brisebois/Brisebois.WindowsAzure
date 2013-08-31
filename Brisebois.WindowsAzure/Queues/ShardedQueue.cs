using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Brisebois.WindowsAzure.Queues
{
public class ShardedQueue
{
    private readonly string queueName;
    private readonly int queueCount;
    private readonly List<CloudStorageAccount> accounts;

    private List<List<CloudQueue>> queues;

    private readonly int accountCount;

    private int lastWriteAccount;
    private int lastWriteQueue;

    private int lastReadAccount;
    private int lastReadQueue;


    public ShardedQueue(string queueName,
                        int queueCount,
                        IEnumerable<CloudStorageAccount> storagAccounts)
    {
        this.queueName = queueName;
        this.queueCount = queueCount;

        accounts = storagAccounts.ToList();

        accountCount = accounts.Count();

        var random = new Random(DateTime.Now.Millisecond);
        lastWriteAccount = random.Next(accountCount);

        var randomQueue = new Random(DateTime.Now.Millisecond);
        lastWriteQueue = randomQueue.Next(queueCount);

        LoadQueues();
    }

    private void LoadQueues()
    {
        var clients = accounts.Select(a => a.CreateCloudQueueClient())
            .ToList();

        queues = clients.Select(c => c.ListQueues()
                                        .Where(q => q.Name.StartsWith(queueName))
                                        .ToList())
                        .ToList();
    }

    public void CreateIfNotExists()
    {
        var clients = accounts.Select(a => a.CreateCloudQueueClient())
                                .ToList();

        foreach (var cloudQueueClient in clients)
            CreateMissingQueues(cloudQueueClient);

        LoadQueues();
    }

    private void CreateMissingQueues(CloudQueueClient cloudQueueClient)
    {
        var shards = cloudQueueClient.ListQueues()
            .Where(q => q.Name.StartsWith(queueName))
            .Select(q => q.Name);

        var queueNames = Enumerable.Range(0, queueCount)
            .Select(i => queueName + i)
            .Where(n => !shards.Contains(n))
            .ToList();

        foreach (var name in queueNames)
        {
            var q = cloudQueueClient.GetQueueReference(name);
            q.CreateIfNotExists();
        }
    }

    public void Delete(ShardedQueueMessage message)
    {
        message.OriginQueue.DeleteMessage(message.Message);
    }

    public IEnumerable<ShardedQueueMessage> Get(int messageCount)
    {
        return Get(messageCount, null);
    }

    public IEnumerable<ShardedQueueMessage> Get(int messageCount,
                                                TimeSpan? visibilityTimeout)
    {
        lock (readLockObject)
        {
            lastReadAccount = (lastReadAccount + 1) % accountCount;
            lastReadQueue = (lastReadQueue + 1) % queueCount;
        }

        var queue = queues[lastReadAccount][lastReadQueue];

        var messages = queue.GetMessages(messageCount, visibilityTimeout)
            .Select(m => new ShardedQueueMessage
            {
                OriginQueue = queue,
                Message = m
            })
            .ToList();

        if (messages.Count > 0)
        {
            var trace = string.Format("Read {2} message to Account {0} Queue {1}",
                lastReadAccount,
                lastReadQueue,
                messages.Count);

            Trace.WriteLine(trace);    
        }    
            
        return messages;
    }

    readonly object readLockObject = new object();
    readonly object writeLockObject = new object();

    public void Add(CloudQueueMessage message)
    {
        lock (writeLockObject)
        {
            lastWriteAccount = (lastWriteAccount + 1) % accountCount;
            lastWriteQueue = (lastWriteQueue + 1) % queueCount;
        }

        var queue = queues[lastWriteAccount][lastWriteQueue];
        queue.AddMessage(message);

        var trace = string.Format("Added message to Account {0} Queue {1}",
                                        lastWriteAccount,
                                        lastWriteQueue);
        Trace.WriteLine(trace);
    }

    public void Add(IEnumerable<CloudQueueMessage> messages)
    {
        messages.AsParallel()
                .ForAll(m =>
        {
            var random = new Random();
            var nextAccount = random.Next(accountCount);

            var randomQueue = new Random();
            var nextQueue = randomQueue.Next(queueCount);

            var queue = queues[nextAccount][nextQueue];
            queue.AddMessage(m);

            var trace = string.Format("Added message to Account {0} Queue {1}",
                                        nextAccount,
                                        nextQueue);
            Trace.WriteLine(trace);
        });
    }
}
}