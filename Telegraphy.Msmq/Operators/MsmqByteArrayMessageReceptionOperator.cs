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
        public MsmqByteArrayMessageReceptionOperator(string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency) :
            this(new LocalSwitchboard(type, concurrencyCount), ".", queueName)
        {

        }
        public MsmqByteArrayMessageReceptionOperator(string machineName, string queueName, LocalConcurrencyType type = DefaultType, uint concurrencyCount = DefaultConncurrency)
            : this(new LocalSwitchboard(type, concurrencyCount), machineName, queueName)
        {
        }

        public MsmqByteArrayMessageReceptionOperator(ILocalSwitchboard switchBoard, string machineName, string queueName)
            : base(switchBoard, machineName, queueName, new Type[] { typeof(byte[]) }, MessageSource.StringMessage)
        {

        }
    }
}
