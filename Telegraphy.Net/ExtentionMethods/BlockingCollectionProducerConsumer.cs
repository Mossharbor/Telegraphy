using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Net
{
    internal class BlockingCollectionProducerConsumer<T> : IProducerConsumerCollection<T>
    {
        IProducerConsumerCollection<T> subCollection;

        public BlockingCollectionProducerConsumer(IProducerConsumerCollection<T> subCollection, int boundedCapacity)
        {
            this.subCollection = subCollection;
            collection = new BlockingCollection<T>(subCollection, boundedCapacity);
        }

        public BlockingCollectionProducerConsumer(int boundedCapacity)
        {
            this.subCollection = null;
            collection = new BlockingCollection<T>(boundedCapacity);
        }

        private BlockingCollection<T> collection = null;

        public int Count { get { return collection.Count; } }

        public bool IsSynchronized { get { return true; } }

        public object SyncRoot
        {
            get { return this; }
        }

        public void CopyTo(T[] array, int index)
        {
            collection.CopyTo(array, index);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetConsumingEnumerable().GetEnumerator();
        }

        public T[] ToArray()
        {
            return collection.ToArray();
        }

        public bool TryAdd(T item)
        {
            return collection.TryAdd(item);
        }

        public bool TryTake(out T item)
        {
            return collection.TryTake(out item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetConsumingEnumerable().GetEnumerator();
        }
    }
}
