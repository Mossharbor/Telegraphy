using global::Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusDeadLetterQueue : ServiceBusQueue
    {
        public ServiceBusDeadLetterQueue(string connectionString, string queue, ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock)
            : base(connectionString, queue, receiveMode, false)
        {
            // For picking up the message from a DLQ, we make a receiver just like for a 
            // regular queue. We could also use QueueClient and a registered handler here. 
            // The required path is constructed with the EntityNameHelper.FormatDeadLetterPath() 
            // helper method, and always follows the pattern "{entity}/$DeadLetterQueue", 
            // meaning that for a queue "Q1", the path is "Q1/$DeadLetterQueue" and for a 
            // topic "T1" and subscription "S1", the path is "T1/Subscriptions/S1/$DeadLetterQueue" 
            this.SubQueue = SubQueue.DeadLetter;
        }

        public ServiceBusDeadLetterQueue(string connectionString, string topic, string subscription, ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock)
            : base(connectionString, topic, subscription, receiveMode, false)
        {
            this.SubQueue = SubQueue.DeadLetter;
        }
    }
}
