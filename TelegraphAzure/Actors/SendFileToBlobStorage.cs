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

namespace Telegraphy.Azure
{
    public class SendFileToBlobStorage : IActor
    {
        CloudBlockBlob blob = null;
        Encoding encoding = Encoding.UTF8;

        public SendFileToBlobStorage(string storageConnectionString, string containerName)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;

            if (!File.Exists(fileName))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileName);
            
            blob.UploadFromFile(fileName);
            return true;
        }
    }
}
