using System;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendFileToAppendBlobStorage : SendToBlobBase, IActor
    {
        public SendFileToAppendBlobStorage(string storageConnectionString, string containerName, Func<string, string> blobTransformNameFcn)
            : base(storageConnectionString, containerName, blobTransformNameFcn)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;
            var blob = container.GetAppendBlobReference(blobTransformNameFcn(fileName));
            SendFile(blob, fileName);
            return true;
        }
    }
}
