using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterStringReceptionOperator : ServiceBusQueueBaseOperator
    {
        const int DefaultDequeueMaxCount = ServiceBusTopicActorMessageReceptionOperator.DefaultDequeueMaxCount;
        const int DefaultConcurrency = ServiceBusTopicActorMessageReceptionOperator.DefaultConcurrency;

        public ServiceBusQueueDeadLetterStringReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), ServiceBusQueueDeadLetterActorMessageDeliveryOperator.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterStringReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, uint concurrency = DefaultConcurrency, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(new LocalSwitchboard(concurrencyType, concurrency), new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, int maxDequeueCount = DefaultDequeueMaxCount)
            : this(switchBoard, ServiceBusQueueDeadLetterActorMessageDeliveryOperator.GetQueue(connectionString, queueName), maxDequeueCount)
        { }

        public ServiceBusQueueDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, int maxDequeueCount = DefaultDequeueMaxCount)
           : this(switchBoard, new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), maxDequeueCount)
        {
        }

        private ServiceBusQueueDeadLetterStringReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount)
            : base(switchBoard, queue, maxDequeueCount, MessageSource.StringMessage)
        {
        }
    }
}
