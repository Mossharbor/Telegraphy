using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class StorageDeadLetterQueuePublishOperator<T> : StorageQueueBaseOperator<T> where T : class
    {
        public StorageDeadLetterQueuePublishOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, DefaultDequeueMaxCount, null)
        {
        }

        public StorageDeadLetterQueuePublishOperator(QueueClient queue, QueueClient deadLetterQueue)
            : base(null, queue, deadLetterQueue, false, DefaultDequeueMaxCount, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
