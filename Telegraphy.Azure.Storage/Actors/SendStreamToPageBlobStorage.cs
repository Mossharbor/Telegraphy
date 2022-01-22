using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Telegraphy.Azure
{
    public class SendStreamToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool createIfNotExist = false;
        long initialSize = 0;

        public SendStreamToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) :
            base(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        public SendStreamToPageBlobStorage(string storageConnectionString, string containerName,bool createIfNotExist, long initialSize,Func<string> blobNameFcn) :
            base(storageConnectionString, containerName, blobNameFcn, null)
        {
            this.initialSize = initialSize;
            this.createIfNotExist = createIfNotExist;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToBlobStorageException();
            var blob = container.GetPageBlobClient(blobNameFcn());
            if (createIfNotExist && initialSize > 0)
                blob.CreateIfNotExists(initialSize);
            this.SendStream(blob, (Stream)msg.Message);
            return true;
        }
    }
}
