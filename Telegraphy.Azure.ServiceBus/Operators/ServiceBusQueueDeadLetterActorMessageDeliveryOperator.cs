using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterActorMessageDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueDeadLetterActorMessageDeliveryOperator(string connectionString, string queueName)
            : base(GetQueue(connectionString, queueName), Telegraphy.Net.MessageSource.EntireIActor)
        { }

        public ServiceBusQueueDeadLetterActorMessageDeliveryOperator(QueueClient queue, string connectionString)
            : base(new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), Telegraphy.Net.MessageSource.EntireIActor)
        { }

        internal static ServiceBusDeadLetterQueue GetQueue(string connectionString, string queueName)
        {
            return new ServiceBusDeadLetterQueue(connectionString, queueName);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
