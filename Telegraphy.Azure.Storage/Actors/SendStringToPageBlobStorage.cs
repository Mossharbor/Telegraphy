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
    public class SendStringToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool checkExistsAndCreate = true;
        long initialSize = 0;

        public SendStringToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) :
            this(storageConnectionString, containerName, false, 0, blobNameFcn, null)
        {
        }

        public SendStringToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn, Encoding encoding)
            : base(storageConnectionString, containerName, blobNameFcn, encoding)
        {
        }

        public SendStringToPageBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, long initialSize, Func<string> blobNameFcn) :
            this(storageConnectionString, containerName, checkExistsAndCreate, initialSize, blobNameFcn, null)
        {
        }

        public SendStringToPageBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, long initialSize, Func<string> blobNameFcn, Encoding encoding)
            : base(storageConnectionString, containerName, blobNameFcn, encoding)
        {
            this.checkExistsAndCreate = checkExistsAndCreate;
            this.initialSize = initialSize;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            var blob = container.GetPageBlobClient(blobNameFcn());
            if (checkExistsAndCreate && 0 != this.initialSize)
                blob.CreateIfNotExists(this.initialSize);
            string msgString = (string)msg.Message;
            SendString(blob, msgString);
            return true;
        }
    }
}
