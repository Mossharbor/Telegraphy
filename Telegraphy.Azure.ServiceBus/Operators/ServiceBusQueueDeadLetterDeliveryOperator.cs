using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterDeliveryOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        public ServiceBusQueueDeadLetterDeliveryOperator(string connectionString, string queueName)
            : base(GetQueue(connectionString, queueName), Telegraphy.Net.MessageSource.StringMessage)
        { }


        public ServiceBusQueueDeadLetterDeliveryOperator(QueueClient queue, string connectionString)
            : base(new ServiceBusDeadLetterQueue(connectionString, queue.QueueName), Telegraphy.Net.MessageSource.StringMessage)
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
