using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.Azure
{
    public class DeleteAllBlobsInContainer : IActor
    {
        CloudBlobClient client = null;
        string prefix = null;
        BlobListingDetails details;
        public DeleteAllBlobsInContainer(string storageConnectionString) :
            this(storageConnectionString, null)
        {
        }

        public DeleteAllBlobsInContainer(string storageConnectionString, string prefix) :
            this(storageConnectionString, prefix, BlobListingDetails.None)
        {
        }

        public DeleteAllBlobsInContainer(string storageConnectionString, string prefix = null, BlobListingDetails details = BlobListingDetails.None)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            client = acct.CreateCloudBlobClient();
            this.prefix = prefix;
            this.details = details;
        }

        void DeleteAllBlobs(IEnumerable<IListBlobItem> items)
        {
            foreach (var blobItem in items)
            {
                if (blobItem.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                    blob.Delete();
                }
                else if (blobItem.GetType() == typeof(CloudAppendBlob))
                {
                    CloudAppendBlob blob = (CloudAppendBlob)blobItem;
                    blob.Delete();
                }
                else if (blobItem.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob blob = (CloudPageBlob)blobItem;
                    blob.Delete();
                }
                else if (blobItem.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory blob = (CloudBlobDirectory)blobItem;
                    DeleteAllBlobs(blob.ListBlobs(false, details));
                }
            }
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteBlobsInContainerContainerNameIsNotAStringException();

            string containername = (string)(msg as IActorMessage).Message;
            var container = client.GetContainerReference(containername);
            DeleteAllBlobs(container.ListBlobs(prefix, false, details));
            return true;
        }
    }
}
