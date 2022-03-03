using global::Azure.Messaging.ServiceBus;
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

        internal static ServiceBusQueue GetQueue(string connectionString, string queueName)
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
