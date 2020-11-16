using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using Microsoft.Azure.Storage.Queue;
    using Telegraphy.Net;

    internal class QueuedCloudMessage : SimpleMessage<CloudQueueMessage>
    {
        public QueuedCloudMessage(CloudQueueMessage message) : base(message)
        {
        }
    }
}
