using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class DirectoryQueueSubscriptionOperator<T> : DirectoryQueueBaseOperator<T> where T:class
    {
        public DirectoryQueueSubscriptionOperator(string queueDirectoryRoot, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), queueDirectoryRoot, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryQueueSubscriptionOperator(LocalConcurrencyType concurrencyType, string queueDirectoryRoot, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), queueDirectoryRoot, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryQueueSubscriptionOperator(ILocalSwitchboard switchBoard, string queueDirectoryRoot, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, queueDirectoryRoot, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryQueueSubscriptionOperator(ILocalSwitchboard switchBoard, DirectoryQueue queue, DirectoryQueue deadLetterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null)
            : base(switchBoard, queue, deadLetterQueue, true, maxDequeueCount, retrieveVisibilityTimeout)
        {
        }
    }
}
