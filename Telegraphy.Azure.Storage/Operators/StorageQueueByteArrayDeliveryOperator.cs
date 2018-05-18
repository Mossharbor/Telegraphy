﻿using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class StorageQueueByteArrayDeliveryOperator : StorageQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = StorageQueueBaseOperator.DefaultDequeueMaxCount;

        public StorageQueueByteArrayDeliveryOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, Telegraphy.Net.MessageSource.ByteArrayMessage, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public StorageQueueByteArrayDeliveryOperator(CloudQueue queue, CloudQueue deadLetterQueue)
            : base(null, queue, deadLetterQueue, false, Telegraphy.Net.MessageSource.ByteArrayMessage, DefaultDequeueMaxCount, null, null, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}