using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Msmq
{
    using System.Messaging;
    using System.Transactions;
    using Telegraphy.Msmq.Exceptions;
    using Telegraphy.Net;

    public class SendBytesToMsMessageQueue<MsgType> : IActor
    {
        MessageQueue msmqQueue = null;

        public SendBytesToMsMessageQueue(string queueName)
        {
            msmqQueue = MsmqHelper.GetMsmqQueue(MsmqHelper.CreateMsmqQueueName("", queueName, "SEND"));
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                throw new SendBytesMsMessageQueueCanOnlySendValueTypeByteArrayMessagesException("ValueTypeMessage<byte>");

            var msmqMessage = new System.Messaging.Message((msg as IActorMessage).Message);
            if (Transaction.Current == null)
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Single);
            }
            else
            {
                msmqQueue.Send(msmqMessage, MessageQueueTransactionType.Automatic);
            }
            return true;
        }
    }
}
