﻿using System;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;


namespace Telegraphy.Azure
{
    public class SendFileToAppendBlobStorage : SendAndRecieveBlobBase, IActor
    {
        bool checkExistsAndCreate = true;

        public SendFileToAppendBlobStorage(string storageConnectionString, string containerName, bool checkExistsAndCreate, Func<string, string> blobTransformNameFcn)
            : base(storageConnectionString, containerName, blobTransformNameFcn)
        {
            this.checkExistsAndCreate = checkExistsAndCreate;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;
            var blob = container.GetAppendBlobClient(blobTransformNameFcn(fileName));
            if (checkExistsAndCreate)
                blob.CreateIfNotExists();
            SendFile(blob, fileName);
            return true;
        }
    }
}
