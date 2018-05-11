using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class RecieveStreamFromBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public RecieveStreamFromBlobStorage(string storageConnectionString, string containerName)
            : base(storageConnectionString, containerName, null, null)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new MessageTypeWasNotAFileNameCannotDownloadBlobDataException();

            string blobName = (msg.Message as string);
            var blob = container.GetBlobReference(blobName);
            msg.ProcessingResult = new System.IO.MemoryStream();
            base.RecieveStream(blob, (System.IO.MemoryStream)msg.ProcessingResult);
            return true;
        }
    }
}
