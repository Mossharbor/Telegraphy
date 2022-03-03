
using Azure.Messaging.ServiceBus.Administration;
using global::Azure.Messaging.ServiceBus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    internal class ServiceBusQueue : ServiceBusClient
    {
        string connectionString = null;
        string queueName = null;
        string topicName = null;
        string subscription = null;
        ServiceBusReceiveMode receiveMode;
        ServiceBusAdministrationClient busAdmin = null;

        public ServiceBusQueue(string connectionString, string queueName, ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock, bool createQueueIfItDoesNotExist = true)
            : base(connectionString)
        {
            this.connectionString = connectionString;
            this.queueName = queueName;
            this.receiveMode = receiveMode;
            this.SubQueue = SubQueue.None;
            busAdmin = new ServiceBusAdministrationClient(this.connectionString);

            if (createQueueIfItDoesNotExist && !busAdmin.QueueExistsAsync(queueName).Result.Value)
            {
                busAdmin.CreateQueueAsync(queueName).Wait();
            }
        }

        public ServiceBusQueue(string connectionString, string topicName, string subscription, ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock, bool createQueueIfItDoesNotExist = true)
            : base(connectionString)
        {
            this.connectionString = connectionString;
            this.queueName = topicName;
            this.topicName = topicName;
            this.subscription = subscription;
            this.receiveMode = receiveMode;
            this.SubQueue = SubQueue.None;
            busAdmin = new ServiceBusAdministrationClient(this.connectionString);

            if (createQueueIfItDoesNotExist && !busAdmin.TopicExistsAsync(topicName).Result.Value)
            {
                busAdmin.CreateTopicAsync(topicName).Wait();
            }

            if (createQueueIfItDoesNotExist && !string.IsNullOrEmpty(subscription) && !busAdmin.SubscriptionExistsAsync(topicName, subscription).Result.Value)
            {
                busAdmin.CreateSubscriptionAsync(topicName, subscription).Wait();
            }
                
        }

        public SubQueue SubQueue { get; protected set; }

        public string QueueName { get { return this.queueName; } }

        public string TopicName { get { return this.topicName; } }

        public string Subscription { get { return this.subscription; } }

        public ServiceBusReceiveMode ReceiveMode { get { return this.receiveMode; } }

        public long ApproximateMessageCount
        {
            get
            {
                if (string.IsNullOrEmpty(subscription))
                {
                    if (this.SubQueue == SubQueue.DeadLetter)
                        return this.busAdmin.GetQueueRuntimePropertiesAsync(this.queueName).Result.Value.DeadLetterMessageCount;
                    else if (this.SubQueue == SubQueue.TransferDeadLetter)
                        return this.busAdmin.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.subscription).Result.Value.TransferDeadLetterMessageCount;

                    return this.busAdmin.GetQueueRuntimePropertiesAsync(this.queueName).Result.Value.TotalMessageCount;
                }
                else
                {
                    if (this.SubQueue == SubQueue.DeadLetter)
                        return this.busAdmin.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.subscription).Result.Value.DeadLetterMessageCount;
                    else if (this.SubQueue == SubQueue.TransferDeadLetter)
                        return this.busAdmin.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.subscription).Result.Value.TransferDeadLetterMessageCount;

                    return this.busAdmin.GetSubscriptionRuntimePropertiesAsync(this.topicName, this.subscription).Result.Value.TotalMessageCount;
                }
            }
        }
    }
}
