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
    public class StorageQueueByteArrayReceptionOperator : StorageQueueBaseOperator
    {
        public StorageQueueByteArrayReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.ByteArrayMessage, null, null, null)
        {
        }

        public StorageQueueByteArrayReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, uint concurrency = 1)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.ByteArrayMessage, null, null, null)
        {
        }

        public StorageQueueByteArrayReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.ByteArrayMessage, null, null, null)
        {
        }

        public StorageQueueByteArrayReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, true, MessageSource.ByteArrayMessage, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
