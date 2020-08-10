using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;


namespace Telegraphy.Azure
{
    public class RetrieveFromTableStorage<K>: IActor
    {
        CloudTable table = null;
        public RetrieveFromTableStorage(string storageConnectionString, string tableName)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = acct.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg.Message is ITableEntity))
                throw new CannotRetriveNonITableEntityMessagesFromTableStorageException(msg.Message.GetType().Name);

            string partitionKey = (msg.Message as ITableEntity).PartitionKey;
            string rowKey = (msg.Message as ITableEntity).RowKey;


            TableOperation op = TableOperation.Retrieve(partitionKey, rowKey);
            TableResult result = table.Execute(op);

            foreach(var t in (result.Result as DynamicTableEntity).Properties)
            {
                if (t.Value.PropertyAsObject.GetType() == typeof(K))
                {
                    msg.ProcessingResult = (K)t.Value.PropertyAsObject;
                    break;
                }
            }

            if (null == msg.ProcessingResult)
                msg.ProcessingResult = ((K)result.Result);

            return true;
        }
    }
}
