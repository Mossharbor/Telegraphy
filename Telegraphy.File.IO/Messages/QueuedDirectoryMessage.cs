using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.File.IO
{
    using Telegraphy.Net;

    internal class QueuedDirectoryMessage : SimpleMessage<DirectoryQueueMessage>
    {
        public QueuedDirectoryMessage(DirectoryQueueMessage message) : base(message)
        {
        }
    }
}
