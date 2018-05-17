using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueStringReceptionOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = StorageQueueBaseOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = StorageQueueBaseOperator.DefaultConcurrency;

        public StorageQueueStringReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeueCount, null, null, null)
        {
        }

        public StorageQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeueCount, null, null, null)
        { 
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeueCount, null, null, null)

        {
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, CloudQueue deadletterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, deadletterQueue, true, MessageSource.EntireIActor, maxDequeueCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
