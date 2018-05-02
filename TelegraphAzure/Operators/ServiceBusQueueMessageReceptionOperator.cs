using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    using Mossharbor.AzureWorkArounds.ServiceBus;

    public class ServiceBusQueueMessageReceptionOperator : ServiceBusQueueOperator
    {
        public ServiceBusQueueMessageReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = 1, int maxDequeueCount =3) 
            : this(concurrencyType, ServiceBusQueueMessageDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, concurrency, maxDequeueCount)
        { }

        public ServiceBusQueueMessageReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency =1, int maxDequeueCount = 3) 
            : this(new LocalSwitchboard(concurrencyType, concurrency), queue, connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueMessageReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3) 
            : this(switchBoard, ServiceBusQueueMessageDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueMessageReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue), createQueueIfItDoesNotExist, maxDequeueCount)
        {
        }

        private ServiceBusQueueMessageReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3) 
            : base(switchBoard, queue, maxDequeueCount, createQueueIfItDoesNotExist)
        {
        }
    }
}
