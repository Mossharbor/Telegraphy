using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Mossharbor.AzureWorkArounds.ServiceBus;

    public class ServiceBusQueueDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueDeliveryOperator(string connectionString,string queueName, bool createQueueIfItDoesNotExist) : this(GetQueue(connectionString, queueName), connectionString, createQueueIfItDoesNotExist)
        { }

        public ServiceBusQueueDeliveryOperator(QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist) : base (new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist))
        { }

        internal static QueueClient GetQueue(string connectionString,string queueName)
        {
            return new QueueClient(connectionString, queueName);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
