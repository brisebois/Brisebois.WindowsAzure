using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Brisebois.WindowsAzure.TableStorage
{
    public class DynamicEntity : TableEntity
    {
        private IDictionary<string, EntityProperty> internalProperties = new Dictionary<string, EntityProperty>();

        /// <summary>
        ///     Stored in decending order
        /// </summary>
        public DynamicEntity(string partitionKey, DateTime dateTime)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentNullException("partitionKey");

            PartitionKey = partitionKey.ToUpperInvariant();

            RowKey = string.Format(CultureInfo.InvariantCulture,
                                   "{0}-{1}", DateTime.MaxValue.Subtract(dateTime)
                                                      .TotalMilliseconds
                                                      .ToString(CultureInfo.InvariantCulture),
                                   Guid.NewGuid());
        }

        public DynamicEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public DynamicEntity()
        {
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
                                        OperationContext operationContext)
        {
            internalProperties = properties;
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return internalProperties;
        }

        public void Add(string key, EntityProperty value)
        {
            internalProperties.Add(key, value);
        }

        public void Add(string key, bool value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, byte[] value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, DateTime? value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, DateTimeOffset? value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, double value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, Guid value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, int value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, long value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }

        public void Add(string key, string value)
        {
            internalProperties.Add(key, new EntityProperty(value));
        }
    }
}