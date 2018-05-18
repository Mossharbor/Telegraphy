using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueReceptionOperator<T> : StorageQueueBaseOperator<T> where T:class
    {
        public StorageQueueReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageQueueReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageQueueReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null, null, null)
        {
        }

        public StorageQueueReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, CloudQueue deadLetterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, deadLetterQueue, true, maxDequeueCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
