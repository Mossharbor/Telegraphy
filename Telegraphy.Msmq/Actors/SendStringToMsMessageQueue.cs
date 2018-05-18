using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Telegraphy.Msmq.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class SendStringToMsMessageQueue : IActor
    {
        MessageQueue msmqQueue = null;

        public SendStringToMsMessageQueue(string queueName)
        {
            msmqQueue = MsmqHelper.GetMsmqQueue(MsmqHelper.CreateMsmqQueueName("", queueName, "SEND"));
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToMSMQException();

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
