﻿using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendMessageToStorageQueue<MsgType> : IActor
    {
        QueueClient queue = null;

        public SendMessageToStorageQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = StorageQueueBaseOperator<object>.GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            StorageQueueBaseOperator<IActorMessage>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
