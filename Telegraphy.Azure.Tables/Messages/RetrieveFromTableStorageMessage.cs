using Microsoft.Azure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class RetrieveFromTableStorageMessage : TableEntity, IActorMessage
    {
        public RetrieveFromTableStorageMessage (string primaryKey, string rowKey)
            : base(primaryKey, rowKey)
        {
        }

        public object Message { get { return this; } set {; } }

        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }
    }
}