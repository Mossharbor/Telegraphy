﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Telegraphy.Azure
{
    public class SendStringToBlobStorage : IActor
    {
        CloudBlockBlob blob = null;
        Encoding encoding = Encoding.UTF8;

        public SendStringToBlobStorage(string storageConnectionString, string containerName, string blobName, Encoding encoding = null)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            blob = container.GetBlockBlobReference(blobName);
            if (null != encoding)
                this.encoding = encoding;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string msgString = (string)msg.Message;
            blob.UploadText(msgString, Encoding.UTF8);
            return true;
        }
    }
}