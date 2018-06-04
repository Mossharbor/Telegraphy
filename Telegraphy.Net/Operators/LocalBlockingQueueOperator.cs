using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    public class LocalBlockingQueueOperator : LocalQueueOperator
    {
        IProducerConsumerCollection<IActorMessage> collection;
        int boundedCapacity = 0;

        public LocalBlockingQueueOperator(int boundedCapactiy)
            : this(new ConcurrentQueue<IActorMessage>(), boundedCapactiy, new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread))
        { }

        public LocalBlockingQueueOperator(IProducerConsumerCollection<IActorMessage> collection, int boundedCapactiy) 
            : this(collection, boundedCapactiy, new LocalSwitchboard(LocalConcurrencyType.OneActorPerThread))
        { }

        public LocalBlockingQueueOperator(IProducerConsumerCollection<IActorMessage> collection, int boundedCapactiy, LocalConcurrencyType concurrencyType, uint concurrency = 1) 
            : this(collection, boundedCapactiy, new LocalSwitchboard(concurrencyType, concurrency))
        { }

        public LocalBlockingQueueOperator(IProducerConsumerCollection<IActorMessage> collection, int boundedCapactiy, ILocalSwitchboard switchBoard)
        {
            this.collection = collection;
        }

        protected override IProducerConsumerCollection<IActorMessage> GetMessageContainer()
        {
            return new BlockingCollectionProducerConsumer<IActorMessage>(this.collection, boundedCapacity);
        }
    }
}
