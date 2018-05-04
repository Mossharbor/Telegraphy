using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueStringReceptionOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public StorageQueueStringReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeuCount, null, null, null)
        {
        }

        public StorageQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeuCount, null, null, null)
        { 
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeuCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, maxDequeuCount, null, null, null)

        {
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, CloudQueue deadletterQueue, int maxDequeuCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, deadletterQueue, true, MessageSource.EntireIActor, maxDequeuCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
