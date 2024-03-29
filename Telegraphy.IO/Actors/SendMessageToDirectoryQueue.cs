﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class SendMessageToDirectoryQueue<MsgType> : IActor
    {
        DirectoryQueue queue = null;

        public SendMessageToDirectoryQueue(string queueRootDirectory, string queueName)
            : this(queueRootDirectory, queueName, true)
        {
        }

        public SendMessageToDirectoryQueue(string queueRootDirectory, string queueName, bool createQueueIfItDoesNotExist)
        {
            queue = DirectoryQueueBaseOperator<object>.GetQueueFrom(queueRootDirectory, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            DirectoryQueueBaseOperator<IActorMessage>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
