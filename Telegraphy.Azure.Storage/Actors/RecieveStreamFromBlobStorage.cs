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
        Func<System.IO.Stream> getStreamFunc = () => { return new System.IO.MemoryStream(); };

        public RecieveStreamFromBlobStorage(string storageConnectionString, string containerName)
            : this(storageConnectionString, containerName, null)
        {
        }

        public RecieveStreamFromBlobStorage(string storageConnectionString, string containerName,Func<System.IO.Stream> getStreamFunc)
            : base(storageConnectionString, containerName, null, null)
        {
            if (null != getStreamFunc)
                this.getStreamFunc = getStreamFunc;

        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new MessageTypeWasNotAFileNameCannotDownloadBlobDataException();

            string blobName = (msg.Message as string);
            var blob = container.GetBlobClient(blobName);
            msg.ProcessingResult = getStreamFunc();
            base.RecieveStream(blob, (System.IO.Stream)msg.ProcessingResult);
            (msg.ProcessingResult as System.IO.Stream).Position = 0;
            return true;
        }
    }
}
