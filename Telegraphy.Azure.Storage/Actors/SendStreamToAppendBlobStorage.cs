using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Mossharbor.AzureWorkArounds.Storage;

namespace Telegraphy.Azure
{
    public class SendStreamToAppendBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool checkExistsAndCreate = true;

        public SendStreamToAppendBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, Func<string> blobNameFcn) :
            base(storageConnectionString, containerName, blobNameFcn, null)
        {
            this.checkExistsAndCreate = checkExistsAndCreate;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToBlobStorageException();
            var blob = container.GetAppendBlobReference(blobNameFcn());
            if (checkExistsAndCreate && !blob.Exists())
                blob.CreateOrReplace();
            this.SendStream(blob, (Stream)msg.Message);
            return true;
        }
    }
}
