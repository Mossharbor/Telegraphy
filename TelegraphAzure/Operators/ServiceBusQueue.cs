using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Mossharbor.AzureWorkArounds.ServiceBus;

    internal class ServiceBusQueue : QueueClient
    {
        string connectionString = null;
        public ServiceBusQueue(string connectionString, string entityPath, bool createQueueIfItDoesNotExist, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null)
            : base(connectionString, entityPath, receiveMode, retryPolicy)
        {
            this.connectionString = connectionString;

            if (createQueueIfItDoesNotExist)
                this.CreateIfNotExists();
        }

        public ServiceBusQueue(string connectionString, QueueClient queue, bool createQueueIfItDoesNotExist)
            : this(connectionString, queue.QueueName, createQueueIfItDoesNotExist, queue.ReceiveMode, queue.RetryPolicy)
        {

        }

        public void CreateIfNotExists()
        {
            NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
            QueueDescription qd;
            if (!ns.QueueExists(this.QueueName, out qd))
                ns.CreateQueue(this.QueueName);
        }

        public uint ApproximateMessageCount
        {
            get
            {
                NamespaceManager ns = NamespaceManager.CreateFromConnectionString(connectionString);
                QueueDescription qd;
                ns.QueueExists(this.QueueName, out qd);
                if (0 == (uint)qd.MessageCount)
                    return 0;

                return (uint)qd.CountDetails.ActiveMessageCount;
            }
        }
    }
}
