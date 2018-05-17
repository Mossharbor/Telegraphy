using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.Azure
{
    public class StringTableEntity : TableEntity
    {
        public StringTableEntity(string primaryKey, string rowKey, string data)
            :base(primaryKey, rowKey)
        {
            this.String = data;
        }

        public string String { get; set; }
    }
}
