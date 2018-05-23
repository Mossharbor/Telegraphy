using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueDeadLetterPublishOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        public ServiceBusQueueDeadLetterPublishOperator(string connectionString, string queueName)
            : base(GetQueue(connectionString, queueName))
        { }


        public ServiceBusQueueDeadLetterPublishOperator(QueueClient queue, string connectionString)
            : base(new ServiceBusDeadLetterQueue(connectionString, queue.QueueName))
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
