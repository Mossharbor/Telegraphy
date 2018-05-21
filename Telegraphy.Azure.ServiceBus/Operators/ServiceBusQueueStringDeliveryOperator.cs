using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueStringDeliveryOperator : ServiceBusQueueBaseOperator<string>
    {
        public ServiceBusQueueStringDeliveryOperator(string connectionString, string queueName, bool createQueueIfItDoesNotExist)
            : base(ServiceBusQueueActorMessageDeliveryOperator.GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), Telegraphy.Net.MessageSource.StringMessage)
        { }

        public ServiceBusQueueStringDeliveryOperator(QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist)
            : base(new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), Telegraphy.Net.MessageSource.StringMessage)
        { }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
