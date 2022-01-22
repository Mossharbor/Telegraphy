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
    public class SendFileToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendFileToPageBlobStorage(string storageConnectionString, string containerName, Func<string, string> blobTransformNameFcn)
            : base(storageConnectionString, containerName, blobTransformNameFcn)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;
            var blob = container.GetPageBlobClient(blobTransformNameFcn(fileName));
            SendFile(blob, fileName);
            return true;
        }
    }
}
