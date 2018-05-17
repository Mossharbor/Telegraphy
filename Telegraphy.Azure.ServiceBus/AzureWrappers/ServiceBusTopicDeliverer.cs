using Microsoft.Azure.ServiceBus;
using Mossharbor.AzureWorkArounds.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusTopicDeliverer : Microsoft.Azure.ServiceBus.Core.MessageSender
    {
        string connectionString;
        protected string[] subscriptions = null;
        protected string topic = null;
        public ServiceBusTopicDeliverer(string connectionString, string topicName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy retryPolicy = null)
            :base(connectionString, topicName, retryPolicy)
        {
            this.connectionString = connectionString;
            this.topic = topicName;

            if (createTopicIfItDoesNotExist)
                CreateIfNotExists();
        }

        public ServiceBusTopicDeliverer(string connectionString, string topicName, string[] subscriptionName, bool createTopicIfItDoesNotExist, Microsoft.Azure.ServiceBus.RetryPolicy retryPolicy = null)
            :base(connectionString, topicName, retryPolicy)
        {
            // NOTE you cannot send to a specific subscripton (just a topic) so we are not using EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName)
            this.connectionString = connectionString;
            this.subscriptions = subscriptionName;
            this.topic = topicName;

            if (createTopicIfItDoesNotExist)
                CreateIfNotExists();
        }

        public void CreateIfNotExists()
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            TopicDescription qd;
            if (!ns.TopicExists(this.topic, out qd))
                ns.CreateTopic (this.topic);

            if (null != subscriptions && subscriptions.Any())
            {
                Parallel.ForEach(subscriptions, subscription =>
                {
                    SubscriptionDescription sd;
                    if (!ns.SubscriptionExists(this.topic, subscription, out sd))
                        ns.CreateSubscription(this.topic, subscription);
                });
            }
        }

        public uint ApproximateMessageCount
        {
            get
            {
                NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
                QueueDescription qd;
                ns.QueueExists(this.Path, out qd);
                if (0 == (uint)qd.MessageCount)
                    return 0;

                return (uint)qd.CountDetails.ActiveMessageCount;
            }
        }
    }
}
