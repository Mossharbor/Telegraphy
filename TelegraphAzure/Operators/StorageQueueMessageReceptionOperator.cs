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
    public class StorageQueueMessageReceptionOperator : StorageQueueOperator
    {
        public StorageQueueMessageReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, null, null, null)
        {
        }

        public StorageQueueMessageReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, uint concurrency = 1)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, null, null, null)
        {
        }

        public StorageQueueMessageReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, null, null, null)
        {
        }

        public StorageQueueMessageReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, OperationContext retrievalOperationContext = null) 
            : base(switchBoard, queue, true, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
