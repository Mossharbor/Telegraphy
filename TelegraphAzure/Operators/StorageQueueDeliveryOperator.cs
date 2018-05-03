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
    public class StorageQueueDeliveryOperator : StorageQueueBaseOperator
    {
        public StorageQueueDeliveryOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true) 
            : base (null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, null,null,null)
        {
        }

        public StorageQueueDeliveryOperator(CloudQueue queue) 
            : base(null, queue, false, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
