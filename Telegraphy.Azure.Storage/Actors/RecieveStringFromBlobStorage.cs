using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class RecieveStringFromBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public RecieveStringFromBlobStorage(string storageConnectionString, string containerName,Encoding encoding)
            : base(storageConnectionString, containerName, null, encoding)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new MessageTypeWasNotAFileNameCannotDownloadBlobDataException();

            string blobName = (msg.Message as string);
            var blob = container.GetBlobClient(blobName);
            msg.ProcessingResult = base.RecieveString(blob);
            return true;
        }
    }
}
