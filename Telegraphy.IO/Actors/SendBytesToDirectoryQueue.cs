﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class SendBytesToDirectoryQueue : FileActionBase, IActor
    {
        DirectoryQueue queue = null;

        public SendBytesToDirectoryQueue(string queueRootDirectory, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = DirectoryQueueBaseOperator<object>.GetQueueFrom(queueRootDirectory, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                throw new SendBytesCanOnlySendValueTypeByteArrayMessagesException("ValueTypeMessage<byte>");

            DirectoryQueueBaseOperator<byte[]>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
