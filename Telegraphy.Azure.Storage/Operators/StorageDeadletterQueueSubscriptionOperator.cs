using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageDeadletterQueueSubscriptionOperator<T> : StorageQueueBaseOperator<T> where T : class
    {
        public StorageDeadletterQueueSubscriptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageDeadletterQueueSubscriptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageDeadletterQueueSubscriptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageDeadletterQueueSubscriptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, CloudQueue deadLetterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, deadLetterQueue, true, maxDequeueCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
