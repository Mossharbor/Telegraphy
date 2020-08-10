using Microsoft.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;


namespace Telegraphy.Azure
{
    public enum TableOperationType { Insert, InsertOrMerge, InsertOrReplace}

    public class SendITableEntityToTableStorage : IActor
    {
        CloudTable table = null;
        TableOperationType operation = TableOperationType.InsertOrReplace;
        public SendITableEntityToTableStorage(string storageConnectionString, string tableName, TableOperationType operation)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = acct.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
            this.operation = operation;

        }
        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg.Message is ITableEntity))
                throw new CannotSentNonITableEntityMessagesToTableStorageException(msg.Message.GetType().Name);

            TableOperation op = null;
            switch (operation)
            {
                case TableOperationType.InsertOrReplace:
                    op = TableOperation.InsertOrReplace(msg.Message as ITableEntity);
                    break;

                case TableOperationType.Insert:
                    op = TableOperation.Insert(msg.Message as ITableEntity);
                    break;

                case TableOperationType.InsertOrMerge:
                    op = TableOperation.InsertOrMerge(msg.Message as ITableEntity);
                    break;
            }

            table.Execute(op);
            return true;
        }
    }
}
