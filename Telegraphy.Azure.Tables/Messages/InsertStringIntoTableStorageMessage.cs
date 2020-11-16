using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class InsertStringIntoTableStorageMessage : InsertIntoTableStorageMessage<StringTableEntity>
    {
        public InsertStringIntoTableStorageMessage(string primaryKey, string rowKey, string message)
         : base (new StringTableEntity(primaryKey, rowKey, message))
        { }
        
        public new Type GetType()
        {
            return typeof(InsertStringIntoTableStorageMessage);
        }
    }
}
