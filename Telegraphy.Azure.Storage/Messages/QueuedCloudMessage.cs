using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using global::Azure.Storage.Queues.Models;
    using Telegraphy.Net;

    internal class QueuedCloudMessage : SimpleMessage<BinaryData>
    {
        public QueuedCloudMessage(string message, SendReceipt reciept) : base(new BinaryData(message))
        {
        }

        public QueuedCloudMessage(BinaryData message, SendReceipt reciept) : base(message)
        {
        }
    }
}
