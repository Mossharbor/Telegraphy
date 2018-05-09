using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToBlobStorage : IActor
    {
        CloudBlockBlob blob = null;

        public SendBytesToBlobStorage(string storageConnectionString,string containerName,string blobName)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            blob = container.GetBlockBlobReference(blobName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]")
                && !(msg as IActorMessage).Message.GetType().Name.Equals("Byte"))
                throw new CannotSendNonByteArrayMessagesToBlobStorageException();

            byte[] msgBytes = (byte[])msg.Message;
            blob.UploadFromByteArray(msgBytes, 0, msgBytes.Length);
            return true;

        }
    }
}
