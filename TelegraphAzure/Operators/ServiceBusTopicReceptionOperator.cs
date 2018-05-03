using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusTopicReceptionOperator : ServiceBusTopicBaseOperator
    {
        const int DefaultDequeueMaxCount = 3;
        const int DefaultPrefetchCount = 0;
        const int DefaultConcurrency = 1;
        
        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, uint concurrency = DefaultConcurrency)
            : this(concurrencyType, connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, DefaultDequeueMaxCount, DefaultPrefetchCount, concurrency)
        {
        }

        public ServiceBusTopicReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, uint concurrency = DefaultConcurrency, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(new LocalSwitchboard(concurrencyType, concurrency), GetSender(connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, prefetchCount, policy), maxDequeueCount)
        {
        }

        public ServiceBusTopicReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, int maxDequeueCount = DefaultDequeueMaxCount, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy policy = null)
               : base(switchBoard, GetSender(connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, prefetchCount, policy), maxDequeueCount)
        {
        }
        
        private static ServiceBusTopicReciever GetSender(string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, int prefetchCount = DefaultPrefetchCount, Microsoft.Azure.ServiceBus.RetryPolicy retryPolicy = null, Microsoft.Azure.ServiceBus.ReceiveMode receiveMode = Microsoft.Azure.ServiceBus.ReceiveMode.PeekLock)
        {
            return new ServiceBusTopicReciever(connectionString, topicName, subscriptionName, createQueueIfItDoesNotExist, receiveMode, retryPolicy, prefetchCount);
        }
    }
}
