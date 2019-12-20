using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.File.IO
{
    using Telegraphy.File.IO.Exceptions;
    using Telegraphy.Net;

    public class SendStringToDirectoryQueue : IActor
    {
        DirectoryQueue queue = null;

        public SendStringToDirectoryQueue(string queueRootDirectory, string queueName, bool createQueueIfItDoesNotExist = true)
        {
            queue = DirectoryQueueBaseOperator<object>.GetQueueFrom(queueRootDirectory, queueName, createQueueIfItDoesNotExist);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new SendStringActorCanOnlySendStringMessagesException();

            DirectoryQueueBaseOperator<string>.SerializeAndSend(msg, queue, (string)msg.Message);
            return true;
        }
    }
}
