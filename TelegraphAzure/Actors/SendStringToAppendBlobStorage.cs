using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendStringToAppendBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendStringToAppendBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) :
            this(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        public SendStringToAppendBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn, Encoding encoding)
            : base(storageConnectionString, containerName, blobNameFcn, encoding)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            var blob = container.GetAppendBlobReference(blobNameFcn());
            string msgString = (string)msg.Message;
            SendString(blob, msgString);
            return true;
        }
    }
}
