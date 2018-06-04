using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    public class LocalOperator : LocalQueueOperator
    {
        public LocalOperator() : this(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread))
        { }

        public LocalOperator(LocalConcurrencyType concurrencyType, uint concurrency = 1) : this(new LocalSwitchboard(concurrencyType, concurrency))
        { }

        public LocalOperator(ILocalSwitchboard switchBoard)
        {
        }

        protected override IProducerConsumerCollection<IActorMessage> GetMessageContainer()
        {
            return new ConcurrentBag<IActorMessage>();
        }
    }
}
