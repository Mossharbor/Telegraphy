using Microsoft.Azure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeleteFromTableStorageMessage: TableEntity, IActorMessage
    {
        public DeleteFromTableStorageMessage(string primaryKey, string rowkey)
            : this (primaryKey, rowkey, "*")
        {
        }

        public DeleteFromTableStorageMessage(string primaryKey, string rowkey,string etag)
            : base(primaryKey, rowkey)
        {
            this.ETag = etag;
        }

        public object Message { get { return this; } set {; } }
        public object ProcessingResult { get; set; }
        public TaskCompletionSource<IActorMessage> Status { get; set; }
    }
}
