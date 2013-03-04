using System;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    /// <summary>
    /// Details: http://alexandrebrisebois.wordpress.com/2013/02/08/service-logger-for-windows-azure-roles-using-table-storage-service/
    /// </summary>
    public class Entry : TableEntity
    {
        public Entry() { }

        private Entry(string service, string @event, string details, string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
            Created = DateTime.UtcNow;
            Details = details;
            Event = @event;
            Service = service;
        }

        public string Service { get; set; }
        public DateTime Created { get; set; }
        public string Details { get; set; }
        public string Event { get; set; }

        public static Entry Make(string service, string @event, string details)
        {
            var rowKey = string.Format(CultureInfo.InvariantCulture,
                                       "{0}-{1}",
                                       DateTime
                                            .MaxValue
                                            .Subtract(DateTime.UtcNow)
                                            .TotalMilliseconds
                                            .ToString(CultureInfo.InvariantCulture),
                                       Guid.NewGuid());
            var partitionKey = service;

            return new Entry(service, @event, details, partitionKey, rowKey);
        }
    }
}