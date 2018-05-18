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
        public MsmqStringMessageReceptionOperator(string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency) :
            this(new LocalSwitchboard(type, concurrencyCount), ".", queueName)
        {

        }
        public MsmqStringMessageReceptionOperator(string machineName, string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency)
            : this(new LocalSwitchboard(type, concurrencyCount), machineName, queueName)
        {
        }

        public MsmqStringMessageReceptionOperator(ILocalSwitchboard switchBoard, string machineName, string queueName)
            : base(switchBoard, machineName, queueName, new Type[] { typeof(string) }, MessageSource.StringMessage)
        {

        }
    }
}
