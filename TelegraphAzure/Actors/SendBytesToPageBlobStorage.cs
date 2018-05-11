﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendBytesToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn)
            : base(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]")
                && !(msg as IActorMessage).Message.GetType().Name.Equals("Byte"))
                throw new CannotSendNonByteArrayMessagesToBlobStorageException();

            byte[] msgBytes = (byte[])msg.Message;
            var blob = container.GetPageBlobReference(blobNameFcn());
            SendBytes(blob, msgBytes);
            return true;

        }
    }
}
