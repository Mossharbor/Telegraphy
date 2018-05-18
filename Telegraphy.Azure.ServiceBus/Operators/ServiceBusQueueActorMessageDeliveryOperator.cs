﻿using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class ServiceBusQueueActorMessageDeliveryOperator : ServiceBusQueueBaseOperator
    {
        public ServiceBusQueueActorMessageDeliveryOperator(string connectionString,string queueName, bool createQueueIfItDoesNotExist)
            : base (GetQueue(connectionString, queueName, createQueueIfItDoesNotExist), Telegraphy.Net.MessageSource.EntireIActor)
        { }

        public ServiceBusQueueActorMessageDeliveryOperator(QueueClient queue, string connectionString, bool createQueueIfItDoesNotExist)
            : base (new ServiceBusQueue(connectionString, queue, createQueueIfItDoesNotExist), Telegraphy.Net.MessageSource.EntireIActor)
        { }

        internal static ServiceBusQueue GetQueue(string connectionString,string queueName,bool createQueueIfItDoesNotExist)
        {
            return new ServiceBusQueue(connectionString, queueName, createQueueIfItDoesNotExist);
        }

        public override bool WaitTillEmpty(TimeSpan timeout)
        {
            // we dont have a queue here since the purpose of this class is to poplate a queue.
            return true;
        }
    }
}