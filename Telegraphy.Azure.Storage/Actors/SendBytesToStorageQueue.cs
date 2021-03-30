using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class SendBytesToStorageQueue : IActor
    {
        QueueClient queue = null;

        public SendBytesToStorageQueue(string storageConnectionString, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = StorageQueueBaseOperator<string>.GetQueueFrom(storageConnectionString, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                throw new SendBytesStorageActorCanOnlySendValueTypeByteArrayMessagesException("ValueTypeMessage<byte>");

            StorageQueueBaseOperator<byte[]>.SerializeAndSend(msg, queue);
            return true;
        }
    }
}
