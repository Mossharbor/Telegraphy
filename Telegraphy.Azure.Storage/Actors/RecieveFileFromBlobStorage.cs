using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace Telegraphy.Azure
{
    public class RecieveFileFromBlobStorage : SendAndRecieveBlobBase, IActor
    {
        FileMode mode = FileMode.OpenOrCreate;
        public RecieveFileFromBlobStorage(string storageConnectionString, string containerName, FileMode mode, Func<string,string> blobNameToLocalFilePathFcn)
            : base(storageConnectionString, containerName, blobNameToLocalFilePathFcn)
        {
            this.mode = mode;
        }

        public RecieveFileFromBlobStorage(string storageConnectionString, string containerName, bool overwrite, Func<string, string> blobNameToLocalFilePathFcn)
           : base(storageConnectionString, containerName, blobNameToLocalFilePathFcn)
        {
            if (!overwrite)
                mode = FileMode.CreateNew;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new MessageTypeWasNotAFileNameCannotDownloadBlobDataException();

            string blobName = (msg.Message as string);
            string localName = base.blobTransformNameFcn(blobName); // blobNameToLocalFilePathFcn
            var blob = container.GetBlobClient(blobName);
            base.RecieveFile(blob, localName, mode);
            msg.ProcessingResult = localName;
            return true;
        }
    }
}
