using global::Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public class ServiceBusQueuePublishOperator<T> : ServiceBusQueueBaseOperator<T> where T : class
    {
        public ServiceBusQueuePublishOperator(string connectionString, string queueName, bool createQueueIfItDoesNotExist)
            : base(GetQueue(connectionString, queueName, createQueueIfItDoesNotExist))
        { }

        internal static ServiceBusQueue GetQueue(string connectionString, string queueName, bool createQueueIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, queueName, ServiceBusReceiveMode.PeekLock, createQueueIfItDoesNotExist);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}
