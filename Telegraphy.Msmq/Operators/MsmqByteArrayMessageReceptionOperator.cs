using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqByteArrayMessageReceptionOperator : MsmqBaseOperator
    {
        public MsmqByteArrayMessageReceptionOperator(string queueName) :
            this(".", queueName)
        {

        }
        public MsmqByteArrayMessageReceptionOperator(string machineName, string queueName)
            : base(machineName, queueName, MessageSource.ByteArrayMessage)
        {
        }
    }
}
