﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;


namespace Telegraphy.Azure
{
    public class SendBytesToAppendBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool checkExistsAndCreate = true;

        public SendBytesToAppendBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, Func<string> blobNameFcn)
            : base(storageConnectionString, containerName, blobNameFcn, null)
        {
            this.checkExistsAndCreate = checkExistsAndCreate;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]")
                && !(msg as IActorMessage).Message.GetType().Name.Equals("Byte"))
                throw new CannotSendNonByteArrayMessagesToBlobStorageException();

            byte[] msgBytes = (byte[])msg.Message;
            var blob = container.GetAppendBlobClient(blobNameFcn());
            if (checkExistsAndCreate)
                blob.CreateIfNotExists();
            SendBytes(blob, msgBytes);
            return true;

        }
    }
}
