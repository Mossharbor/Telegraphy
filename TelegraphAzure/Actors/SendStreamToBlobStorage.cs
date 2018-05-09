using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Telegraphy.Azure.Actors
{
    public class SendStreamToBlobStorage : IActor
    {
        CloudBlockBlob blob = null;
        Encoding encoding = Encoding.UTF8;

        public SendStreamToBlobStorage(string storageConnectionString, string containerName, string blobName)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            blob = container.GetBlockBlobReference(blobName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToBlobStorageException();

            Stream fileStream = (Stream)msg.Message;

            blob.UploadFromStream(fileStream);
            return true;
        }
    }
}
