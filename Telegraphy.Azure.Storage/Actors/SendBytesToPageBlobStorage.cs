using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Telegraphy.Azure
{
    public class SendBytesToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool createIfNotexists = false;
        long initialSize = 0;

        public SendBytesToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn)
            : base(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        public SendBytesToPageBlobStorage(string storageConnectionString, string containerName, bool createIfNotexists, long initialSize, Func<string> blobNameFcn)
            : base(storageConnectionString, containerName, blobNameFcn, null)
        {
            this.createIfNotexists = createIfNotexists;
            this.initialSize = initialSize;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]")
                && !(msg as IActorMessage).Message.GetType().Name.Equals("Byte"))
                throw new CannotSendNonByteArrayMessagesToBlobStorageException();

            byte[] msgBytes = (byte[])msg.Message;
            var blob = container.GetPageBlobClient(blobNameFcn());
            if (this.createIfNotexists && this.initialSize > 0)
                blob.CreateIfNotExists(this.initialSize);

            SendBytes(blob, msgBytes);
            return true;

        }
    }
}
