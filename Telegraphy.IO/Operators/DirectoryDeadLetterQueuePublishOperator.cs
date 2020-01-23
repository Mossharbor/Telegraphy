using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.IO
{
    public class DirectoryDeadLetterQueuePublishOperator<T> : DirectoryQueueBaseOperator<T> where T : class
    {
        public DirectoryDeadLetterQueuePublishOperator(string queueDirectoryRoot, string queueName, bool createQueueIfItDoesNotExist = true)
            : base(null, queueDirectoryRoot, queueName, createQueueIfItDoesNotExist, false, DefaultDequeueMaxCount, null)
        {
        }

        public DirectoryDeadLetterQueuePublishOperator(DirectoryQueue queue, DirectoryQueue deadLetterQueue)
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
