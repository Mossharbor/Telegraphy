﻿using System;
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
    public class SendFileToBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendFileToBlobStorage(string storageConnectionString, string containerName, Func<string,string> blobTransformNameFcn)
            :base(storageConnectionString, containerName, blobTransformNameFcn)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToBlobStorageException();

            string fileName = (string)msg.Message;
            string blobFileName = blobTransformNameFcn(fileName);
            var blob = container.GetBlockBlobReference(blobTransformNameFcn(fileName));
            SendFile(blob, fileName);
            msg.ProcessingResult = blobFileName;
            return true;
        }
    }
}