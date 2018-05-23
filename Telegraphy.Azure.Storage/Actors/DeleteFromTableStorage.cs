using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;
using Mossharbor.AzureWorkArounds.Storage;

namespace Telegraphy.Azure
{
    public class DeleteFromTableStorage : IActor
    {
        CloudTable table = null;
        public DeleteFromTableStorage(string storageConnectionString, string tableName)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = acct.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg.Message is ITableEntity))
                throw new CannotDeleteNonITableEntityMessagesFromTableStorageException(msg.Message.GetType().Name);
            
            TableOperation op = TableOperation.Delete((msg.Message as ITableEntity));
            TableResult result = table.Execute(op);
            return true;
        }
    }
}
