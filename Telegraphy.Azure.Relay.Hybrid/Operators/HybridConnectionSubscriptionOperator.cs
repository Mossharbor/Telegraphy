using Microsoft.Azure.Relay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.Net.TPLExtentions;

namespace Telegraphy.Azure.Relay.Hybrid
{
    public class HybridConnectionSubscriptionOperator<MsgType> : HybridConnectionBaseOperator<MsgType> where MsgType : class
    {
        public HybridConnectionSubscriptionOperator(string hybridConnectionString, bool createConnectionIfItDoesNotExist = true) : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), hybridConnectionString, createConnectionIfItDoesNotExist)
        {
        }
        public HybridConnectionSubscriptionOperator(string hybridConnectionString, string hybridConnectionName, bool createConnectionIfItDoesNotExist = true) : base(new LocalSwitchboard(LocalConcurrencyType.OneThreadAllActors), hybridConnectionString, hybridConnectionName, createConnectionIfItDoesNotExist)
        {
        }

        public HybridConnectionSubscriptionOperator(LocalConcurrencyType concurrencyType, string hybridConnectionString, bool createConnectionIfItDoesNotExist = true, uint concurrency = DefaultConcurrency) : base(new LocalSwitchboard(concurrencyType, concurrency), hybridConnectionString, createConnectionIfItDoesNotExist)
        {
        }
        public HybridConnectionSubscriptionOperator(LocalConcurrencyType concurrencyType, string hybridConnectionString, string hybridConnectionName, bool createConnectionIfItDoesNotExist = true, uint concurrency = DefaultConcurrency) : base(new LocalSwitchboard(concurrencyType, concurrency), hybridConnectionString, hybridConnectionName, createConnectionIfItDoesNotExist)
        {
        }

        public HybridConnectionSubscriptionOperator(ILocalSwitchboard switchBoard, string hybridConnectionString, bool createConnectionIfItDoesNotExist = true) : base(switchBoard, hybridConnectionString, createConnectionIfItDoesNotExist)
        {
        }
        public HybridConnectionSubscriptionOperator(ILocalSwitchboard switchBoard, string hybridConnectionString, string hybridConnectionName, bool createConnectionIfItDoesNotExist = true) : base(switchBoard, hybridConnectionString, hybridConnectionName, createConnectionIfItDoesNotExist)
        {
        }
    }
}
