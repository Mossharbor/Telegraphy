﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO
{
    public class DirectoryQueuePublishOperator<T> : DirectoryQueueBaseOperator<T> where T:class
    {
        public DirectoryQueuePublishOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(null, storageConnectionString, queueName, createQueueIfItDoesNotExist, false, DefaultDequeueMaxCount, null)
        {
        }

        public DirectoryQueuePublishOperator(DirectoryQueue queue, DirectoryQueue deadLetterQueue)
            : base(null, queue, deadLetterQueue, false, DefaultDequeueMaxCount, null)
        {
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
