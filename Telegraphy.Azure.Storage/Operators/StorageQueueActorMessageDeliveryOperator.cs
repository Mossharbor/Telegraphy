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
    public class StorageQueueActorMessageDeliveryOperator : StorageQueueBaseOperator
    { 
        const int DefaultDequeueMaxCount = StorageQueueBaseOperator.DefaultDequeueMaxCount;

        public StorageQueueActorMessageDeliveryOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true) 
            : base (null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, MessageSource.EntireIActor, DefaultDequeueMaxCount, null, null,null)
        {
        }

        public StorageQueueActorMessageDeliveryOperator(CloudQueue queue, CloudQueue deadLetterQueue) 
            : base(null, queue, deadLetterQueue, false, MessageSource.EntireIActor, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
