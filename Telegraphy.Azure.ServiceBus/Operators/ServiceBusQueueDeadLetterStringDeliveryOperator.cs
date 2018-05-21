using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterStringDeliveryOperator : ServiceBusQueueBaseOperator<string>
    {
        public ServiceBusQueueDeadLetterStringDeliveryOperator(string connectionString, string queueName)
            : base(ServiceBusQueueDeadLetterActorMessageDeliveryOperator.GetQueue(connectionString, queueName), Telegraphy.Net.MessageSource.StringMessage)
        { }

        public ServiceBusQueueDeadLetterStringDeliveryOperator(QueueClient queue, string connectionString)
            : base(new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), Telegraphy.Net.MessageSource.StringMessage)
        { }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
