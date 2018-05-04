using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    class ServiceBusDeadLetterQueue : ServiceBusQueue
    {
        public ServiceBusDeadLetterQueue(string connectionString, string entityPath, ReceiveMode receiveMode = ReceiveMode.PeekLock, RetryPolicy retryPolicy = null)
            : base(connectionString, EntityNameHelper.FormatDeadLetterPath(entityPath), false, receiveMode, retryPolicy)
        {
        }
    }
}
