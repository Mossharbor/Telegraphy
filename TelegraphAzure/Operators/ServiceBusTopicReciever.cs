using Microsoft.Azure.ServiceBus;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusTopicReciever : Microsoft.Azure.ServiceBus.Core.MessageReceiver
    {
        string connectionString;
        string subscription = null;
        string topic = null;

        public ServiceBusTopicReciever(string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null, int prefetchCount = 0)
            : base(connectionString, EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName), receiveMode, retryPolicy, prefetchCount)
        {
            this.connectionString = connectionString;
            this.topic = topicName;
            this.subscription = subscriptionName;

            if (createQueueIfItDoesNotExist)
                this.CreateIfNotExists();
        }

        public void CreateIfNotExists()
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            TopicDescription qd;
            if (!ns.TopicExists(this.topic, out qd))
                ns.CreateTopic(this.topic);

            if (!String.IsNullOrEmpty(this.subscription))
            {
                SubscriptionDescription sd;
                if (!ns.SubscriptionExists(this.topic, this.subscription, out sd))
                    ns.CreateSubscription(this.topic, this.subscription);
            }
        }

        public uint ApproximateMessageCount
        {
            get
            {
                NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);

                if (!String.IsNullOrEmpty(this.subscription))
                {
                    SubscriptionDescription sd;
                    ns.SubscriptionExists(this.topic, this.subscription, out sd);
                    return (uint)sd.MessageCount;
                }
                return 0;
            }
        }
    }
}
