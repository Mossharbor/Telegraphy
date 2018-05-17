using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class StorageQueueDeadLetterByteArrayDeliveryOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = StorageQueueBaseOperator.DefaultDequeueMaxCount;

        public StorageQueueDeadLetterByteArrayDeliveryOperator(string storageConnectionString, string queueName)
            : this(StorageQueueBaseOperator.GetDeadLetterQueueFrom(storageConnectionString, queueName))
        {
        }

        public StorageQueueDeadLetterByteArrayDeliveryOperator(CloudQueue deadLetterQueue)
            : base(null, deadLetterQueue, null, false, Telegraphy.Net.MessageSource.ByteArrayMessage, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
