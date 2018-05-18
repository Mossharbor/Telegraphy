using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class SendMessageToMsMessageQueue : IActor
    {
        MessageQueue msmqQueue = null;

        public SendMessageToMsMessageQueue(string queueName)
        {
            msmqQueue = MsmqHelper.GetMsmqQueue(MsmqHelper.CreateMsmqQueueName("", queueName, "SEND"));
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            var msmqMessage = new System.Messaging.Message(msg);
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
