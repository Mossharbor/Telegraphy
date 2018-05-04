using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class StorageQueueStringDeliveryOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;

        public StorageQueueStringDeliveryOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, MessageSource.StringMessage, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public StorageQueueStringDeliveryOperator(CloudQueue queue, CloudQueue deadletterQueue)
            : base(null, queue, deadletterQueue, false, MessageSource.StringMessage, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
