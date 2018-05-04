using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueActorMessageReceptionOperator : StorageQueueBaseOperator
    {
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;

        public StorageQueueActorMessageReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.EntireIActor, maxDequeuCount, null, null, null)
        {
        }

        public StorageQueueActorMessageReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.EntireIActor, maxDequeuCount, null, null, null)
        {
        }

        public StorageQueueActorMessageReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.EntireIActor, maxDequeuCount, null, null, null)
        {
        }

        public StorageQueueActorMessageReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, CloudQueue deadLetterQueue, int maxDequeuCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null) 
            : base(switchBoard, queue, deadLetterQueue, true, MessageSource.EntireIActor, maxDequeuCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
