﻿using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueStringReceptionOperator : StorageQueueBaseOperator
    {
        public StorageQueueStringReceptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, null, null, null)
        {
        }

        public StorageQueueStringReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, uint concurrency = 1)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, null, null, null)
        {
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, MessageSource.StringMessage, null, null, null)

        {
        }

        public StorageQueueStringReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue queue, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, queue, true, MessageSource.EntireIActor, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
