using Microsoft.Azure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class InsertIntoTableStorageMessage<T> : IActorMessage  where T:ITableEntity
    {
        public InsertIntoTableStorageMessage(T message)
        {
            this.Message = message;
        }
        public object Message { get; set; }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }
    }
}
