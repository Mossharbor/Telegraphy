﻿using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class StorageQueueDeadLetterStringReceptionOperator : StorageQueueBaseOperator<string>
    {
        public StorageQueueDeadLetterStringReceptionOperator(string storageConnectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
                 : this(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), StorageQueueBaseOperator<string>.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterStringReceptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : this(new LocalSwitchboard(concurrencyType, concurrency), StorageQueueBaseOperator<string>.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
               : this(switchBoard, StorageQueueBaseOperator<string>.GetDeadLetterQueueFrom(storageConnectionString, queueName), maxDequeueCount)
        {
        }

        public StorageQueueDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, CloudQueue deadletterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null, QueueRequestOptions retrievalRequestOptions = null, Microsoft.WindowsAzure.Storage.OperationContext retrievalOperationContext = null)
            : base(switchBoard, deadletterQueue, null, true, maxDequeueCount, retrieveVisibilityTimeout, retrievalRequestOptions, retrievalOperationContext)
        {
        }
    }
}
