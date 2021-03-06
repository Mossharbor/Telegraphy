﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    using global::Azure.Storage.Queues;
    using Telegraphy.Azure.Exceptions;
    using Telegraphy.Net;

    public class SendStringToStorageQueue : IActor
    {
        QueueClient queue = null;

        public SendStringToStorageQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = StorageQueueBaseOperator<object>.GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new SendStringActorCanOnlySendStringMessagesException();

            StorageQueueBaseOperator<string>.SerializeAndSend(msg, queue, (string)msg.Message);
            return true;
        }
    }
}
