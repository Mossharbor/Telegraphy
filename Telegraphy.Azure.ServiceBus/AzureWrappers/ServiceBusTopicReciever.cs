using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusTopicReciever : MessageReceiver
    {
        string connectionString;
        protected string subscription = null;
        protected string topic = null;
        string entityPath = null;

        public ServiceBusTopicReciever(string connectionString, string topicName, string subscriptionName, bool createQueueIfItDoesNotExist, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null, int prefetchCount = 0)
            : this(connectionString, EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName), createQueueIfItDoesNotExist, receiveMode, retryPolicy, prefetchCount)
        {
            this.topic = topicName;
            this.subscription = subscriptionName;
        }

        public ServiceBusTopicReciever(string connectionString, string entityPath, bool createQueueIfItDoesNotExist, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null, int prefetchCount = 0)
            : base(connectionString, entityPath, receiveMode, retryPolicy, prefetchCount)
        {
            this.entityPath = entityPath;
            this.connectionString = connectionString;

            if (createQueueIfItDoesNotExist)
                this.CreateIfNotExists();
        }

        public void CreateIfNotExists()
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!ns.TopicExists(this.topic))
                ns.CreateTopic(this.topic);

            if (!String.IsNullOrEmpty(this.subscription))
            {
                if (!ns.SubscriptionExists(this.topic, this.subscription))
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
                    var sd = ns.GetSubscription(this.topic, this.subscription);
                    return (uint)sd.MessageCount;
                }
                return 0;
            }
        }
    }
}
