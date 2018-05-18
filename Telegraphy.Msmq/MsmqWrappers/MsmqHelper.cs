using System;

namespace Telegraphy.Msmq
{
    using System.Messaging;

    public class MsmqHelper
    { // Create the specified transactional MSMQ queue if it doesn't exist.
        // If it exists, open existing queue. Return the queue handle.
        public static MessageQueue GetMsmqQueue(string queueName)
        {
            var msmqQueue = new MessageQueue(queueName, true);
            if (!MessageQueue.Exists(queueName))
            {
                MessageQueue.Create(queueName, true);
            }
            else
            {
                msmqQueue.Refresh();
            }
            msmqQueue.MessageReadPropertyFilter.SetAll();
            msmqQueue.Formatter = new XmlMessageFormatter(new[] { typeof(Message) });
            return msmqQueue;
        }

        // Create an MSMQ queue.
        public static string CreateMsmqQueueName(string prefix, string queueName, string suffix, string machineName=".")
        {
            return (machineName+"\\private$\\" + prefix.Replace(".", "_") + queueName.Replace("/", "_") + "_" + suffix);
        }
    }
}
