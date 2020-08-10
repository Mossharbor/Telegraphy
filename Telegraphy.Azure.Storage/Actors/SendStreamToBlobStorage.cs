using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Microsoft.Azure.Storage;

namespace Telegraphy.Azure
{
    public class SendStreamToBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendStreamToBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) :
            base(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToBlobStorageException();
            var blob = container.GetBlockBlobReference(blobNameFcn());
            this.SendStream(blob, (Stream)msg.Message);
            return true;
        }
    }
}
