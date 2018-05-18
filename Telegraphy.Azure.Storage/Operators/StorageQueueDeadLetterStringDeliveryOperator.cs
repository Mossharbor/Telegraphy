using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class StorageQueueDeadLetterStringDeliveryOperator : StorageQueueBaseOperator<string>
    {
        public StorageQueueDeadLetterStringDeliveryOperator(string storageConnectionString, string queueName)
            : this(StorageQueueBaseOperator<string>.GetDeadLetterQueueFrom(storageConnectionString, queueName))
        {
        }

        public StorageQueueDeadLetterStringDeliveryOperator(CloudQueue deadLetterQueue)
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
