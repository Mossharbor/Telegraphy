using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class InsertBytesIntoTableStorageMessage : InsertIntoTableStorageMessage<BytesTableEntity>
    {
        public InsertBytesIntoTableStorageMessage(string primaryKey, string rowKey, byte[] message)
         : base(new BytesTableEntity(primaryKey, rowKey, message))
        { }

        public new Type GetType()
        {
            return typeof(InsertBytesIntoTableStorageMessage);
        }
    }
}
