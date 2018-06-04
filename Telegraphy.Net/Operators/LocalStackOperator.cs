using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    public class LocalStackOperator : LocalQueueOperator
    {
        public LocalStackOperator() : this(new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread))
        { }

        public LocalStackOperator(LocalConcurrencyType concurrencyType, uint concurrency = 1) : this(new LocalSwitchboard(concurrencyType, concurrency))
        { }

        public LocalStackOperator(ILocalSwitchboard switchBoard)
        {
        }

        protected override IProducerConsumerCollection<IActorMessage> GetMessageContainer()
        {
            return new ConcurrentStack<IActorMessage>();
        }
    }
}
