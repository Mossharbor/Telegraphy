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
        bool checkExistsAndCreate = true;

        public SendStringToAppendBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, Func<string> blobNameFcn) :
            this(storageConnectionString, containerName, checkExistsAndCreate, blobNameFcn, null)
        {
        }

        public SendStringToAppendBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, Func<string> blobNameFcn, Encoding encoding)
            : base(storageConnectionString, containerName, blobNameFcn, encoding)
        {
            this.checkExistsAndCreate = checkExistsAndCreate;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            var blob = container.GetAppendBlobReference(blobNameFcn());
            if (checkExistsAndCreate && !blob.Exists())
                blob.CreateOrReplace();
            string msgString = (string)msg.Message;
            SendString(blob, msgString);
            return true;
        }
    }
}
