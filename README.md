Disclaimer

These are my personal Windows Azure tools. I am actively adding to them as I publish new blog posts.
This code does not come with any guaranties. Bugs are possible and I will be happy to fix bugs that are reported through the dedicated Issues page on the GitHub repository. Please keep in mind that work on this code is done on me personal time.
For those who are wish to contribute bug fixes and new features, I will be happy to consider all pull requests and collaborate in order to integrate your  generous contributions to the package available on Nuget.org.
 
Contents

Logging & Diagnostics
Logger – A simple lightweight logger. The problem I have with more widely used Logging frameworks, is that they collect too much information. This Logger has helped me rapidly stop errors in my Windows Azure Worker Roles.

Windows Azure SQL Database
ReliableBulkWriter – SQL Database on Windows Azure is extremely good at protecting itself from abuse. This bulk writer has helped me insert massive amounts of data into SQL Database.
ReliableModel – Makes working with Entity Framework and Windows Azure SQL Database simpler by wrapping modifications with a transaction and by using the exponential back off transient error detection strategy. This is necessary because the connection to SQL Database is not considered to be reliable.

Windows Azure Blob Storage Service
BlobCompressor – Keep blobs in a container compressed. This is an example of what can be achieved with a BlobContainerWorker. The BlobCompressor monitors a container and compresses blobs that are not compressed. Upload new blobs and they will be compressed within a 17 minute delay.
BlobContainerWorker – Is a base class that can be implemented to work with blobs in a container. It implements a PollingTask.

Windows Azure Queue Storage Service
QueueWorker – Is a base class that can be implemented to work with queues. It implements a PollingTask.

REST
HttpTransientErrorDetectionStrategy – Adds logic to decide weather the exception raised by the code executed within a RetryPolicy is a transient HTTP error.
RestClient – A Fluent REST Client that uses the HttpTransientErrorDetectionStrategy and a RetryPolicy to be tolerant to transient faults.

Framework
PollingTask – Is a base class responsible for limiting costs by backing off from services when there is no work to be done. They are building blocks for my Worker Roles and empower me to easily organize work loads.
DelayCalculator – Used to calculate an exponential back off delay based on failed attempts.
