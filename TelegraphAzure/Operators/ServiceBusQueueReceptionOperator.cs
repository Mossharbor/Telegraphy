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

    public class ServiceBusQueueReceptionOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueReceptionOperator(LocalConcurrencyType concurrencyType, string connectionString, string queueName, bool createQueueIfItDoesNotExist, uint concurrency = 1, int maxDequeueCount =3) 
            : this(concurrencyType, ServiceBusQueueDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, concurrency, maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(LocalConcurrencyType concurrencyType, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, uint concurrency =1, int maxDequeueCount = 3) 
            : this(new LocalSwitchboard(concurrencyType, concurrency), queue, connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, string connectionString, string queueName, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3) 
            : this(switchBoard, ServiceBusQueueDeliveryOperator.GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist, maxDequeueCount)
        { }

        public ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist, int maxDequeueCount = 3)
           : this(switchBoard, new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), maxDequeueCount)
        {
        }

        private ServiceBusQueueReceptionOperator(ILocalSwitchboard switchBoard, ServiceBusQueue queue, int maxDequeueCount = 3) 
            : base(switchBoard, queue, maxDequeueCount)
        {
        }
    }
}
