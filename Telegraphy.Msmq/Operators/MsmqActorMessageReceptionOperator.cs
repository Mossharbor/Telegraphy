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
        public MsmqActorMessageReceptionOperator(string queueName, Type[] targetTypes, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency) :
            this(new LocalSwitchboard(type, concurrencyCount), ".", queueName, targetTypes)
        {

        }
        public MsmqActorMessageReceptionOperator(string machineName, string queueName, Type[] targetTypes, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency)
            : this(new LocalSwitchboard(type, concurrencyCount), machineName, queueName, targetTypes)
        {
        }

        public MsmqActorMessageReceptionOperator(ILocalSwitchboard switchBoard, string machineName, string queueName, Type[] targetTypes)
            : base(switchBoard, machineName, queueName, targetTypes, MessageSource.StringMessage)
        {

        }
    }
}
