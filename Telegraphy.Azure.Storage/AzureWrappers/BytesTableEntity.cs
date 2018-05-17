using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class BytesTableEntity : TableEntity
    {
        public BytesTableEntity(string primaryKey, string rowKey, byte[] data)
            : base(primaryKey, rowKey)
        {
            this.Bytes = data;
        }

        public byte[] Bytes { get; set; }
    }
}
