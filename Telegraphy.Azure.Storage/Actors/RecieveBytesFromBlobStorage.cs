using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class RecieveBytesFromBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public RecieveBytesFromBlobStorage(string storageConnectionString, string containerName)
            : base(storageConnectionString, containerName, null,  null)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new MessageTypeWasNotAFileNameCannotDownloadBlobDataException();

            string blobName = (msg.Message as string);
            var blob = container.GetBlobReference(blobName);
            int size = 0;
            msg.ProcessingResult = base.RecieveBytes(blob, out size);
            return true;
        }
    }
}
