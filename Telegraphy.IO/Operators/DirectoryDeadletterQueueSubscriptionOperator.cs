using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class DirectoryDeadletterQueueSubscriptionOperator<T> : DirectoryQueueBaseOperator<T> where T : class
    {
        public DirectoryDeadletterQueueSubscriptionOperator(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
                 : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryDeadletterQueueSubscriptionOperator(LocalConcurrencyType concurrencyType, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount, uint concurrency = DefaultConcurrency)
                 : base(new LocalSwitchboard(concurrencyType, concurrency), storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryDeadletterQueueSubscriptionOperator(ILocalSwitchboard switchBoard, string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true, int maxDequeueCount = DefaultDequeueMaxCount)
               : base(switchBoard, storageConnectionString, queueName, createQueueIfItDoesNotExist, true, maxDequeueCount, null)
        {
        }

        public DirectoryDeadletterQueueSubscriptionOperator(ILocalSwitchboard switchBoard, DirectoryQueue queue, DirectoryQueue deadLetterQueue, int maxDequeueCount = DefaultDequeueMaxCount, TimeSpan? retrieveVisibilityTimeout = null)
            : base(switchBoard, queue, deadLetterQueue, true, maxDequeueCount, retrieveVisibilityTimeout)
        {
        }
    }
}
