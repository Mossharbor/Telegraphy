using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqActorMessageReceptionOperator : MsmqBaseOperator
    {
        public MsmqActorMessageReceptionOperator(string queueName) :
            this(".", queueName)
        {

        }
        public MsmqActorMessageReceptionOperator(string machineName, string queueName)
            : base(machineName, queueName, MessageSource.EntireIActor)
        {
        }
    }
}
