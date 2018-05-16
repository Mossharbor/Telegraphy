﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendStreamToPageBlobStorage : SendAndRecieveBlobBase, IActor
    {
        public SendStreamToPageBlobStorage(string storageConnectionString, string containerName, Func<string> blobNameFcn) :
            base(storageConnectionString, containerName, blobNameFcn, null)
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToBlobStorageException();
            var blob = container.GetPageBlobReference(blobNameFcn());
            this.SendStream(blob, (Stream)msg.Message);
            return true;
        }
    }
}