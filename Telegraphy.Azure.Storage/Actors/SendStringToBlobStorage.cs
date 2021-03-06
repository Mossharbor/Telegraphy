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
    public class SendStringToBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendStringToBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) : 
            this(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        public SendStringToBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn, Encoding encoding)
            :base (storageConnectionString, containerName, blobNameFcn, encoding)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            var blob = container.GetBlockBlobClient(blobNameFcn());
            string msgString = (string)msg.Message;
            SendString(blob, msgString);
            return true;
        }
    }
}
