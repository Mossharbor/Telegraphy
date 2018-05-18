using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqStringMessageReceptionOperator : MsmqBaseOperator
    {
        public MsmqStringMessageReceptionOperator(string queueName) :
            this(".", queueName)
        {

        }
        public MsmqStringMessageReceptionOperator(string machineName, string queueName)
            : base(machineName, queueName, MessageSource.ByteArrayMessage)
        {
        }
    }
}
