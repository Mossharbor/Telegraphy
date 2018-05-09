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
        CloudBlobContainer container = null;
        CloudBlockBlob blob = null;
        Encoding encoding = Encoding.UTF8;
        Func<string, string> blobNameFcn;

        public SendFileToBlobStorage(string storageConnectionString, string containerName, Func<string,string> blobNameFcn)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            container = client.GetContainerReference(containerName);
            this.blobNameFcn = blobNameFcn;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;

            if (!File.Exists(fileName))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileName);

            blob = container.GetBlockBlobReference(blobNameFcn(fileName));
            blob.UploadFromFile(fileName);
            return true;
        }
    }
}
