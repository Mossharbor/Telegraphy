using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Msmq
{
    public class MsmqReceptionOperator<T> : MsmqBaseOperator<T> where T:class
    {
        public MsmqReceptionOperator(string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency) :
            this(new LocalSwitchboard(type, concurrencyCount), ".", queueName)
        {

        }
        public MsmqReceptionOperator(string machineName, string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency)
            : this(new LocalSwitchboard(type, concurrencyCount), machineName, queueName)
        {
        }

        public MsmqReceptionOperator(ILocalSwitchboard switchBoard, string machineName, string queueName)
            : base(switchBoard, machineName, queueName)
        {

        }
    }
}
