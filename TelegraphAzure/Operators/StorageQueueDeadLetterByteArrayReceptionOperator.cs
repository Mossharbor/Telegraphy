using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueDeadLetterByteArrayReceptionOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public StorageQueueDeadLetterByteArrayReceptionOperator(string storageConnectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
                 : this(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), StorageQueueBaseOperator.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName,  int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : this(new LocalSwitchboard(concurrencyType, concurrency), StorageQueueBaseOperator.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterByteArrayReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName,int maxDequeueCount = DefaultDequeueMaxCount)
               : this(switchBoard, StorageQueueBaseOperator.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterByteArrayReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue deadletterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, deadletterQueue, null, true, MessageSource.ByteArrayMessage, maxDequeueCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
