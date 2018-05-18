using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueDeadLetterActorMessageDeliveryOperator : StorageQueueBaseOperator<IActorMessage>
    {
        public StorageQueueDeadLetterActorMessageDeliveryOperator(string storageConnectionString, string queueName)
            : this(StorageQueueBaseOperator<IActorMessage>.GetDeadLetterQueueFrom(storageConnectionString, queueName))
        {
        }

        public StorageQueueDeadLetterActorMessageDeliveryOperator(CloudQueue deadLetterQueue)
            : base(null, deadLetterQueue, null, false, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
