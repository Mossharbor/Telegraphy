using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
        BlobServiceClient client = null;
        string prefix = null;
        BlobStates blobStates;
        public DeleteAllBlobsInContainer(string storageConnectionString) :
            this(storageConnectionString, null)
        {
        }

        public DeleteAllBlobsInContainer(string storageConnectionString, string prefix) :
            this(storageConnectionString, prefix, BlobStates.None)
        {
        }

        public DeleteAllBlobsInContainer(string storageConnectionString, string prefix = null, BlobStates details = BlobStates.None)
        {
            client = new BlobServiceClient(storageConnectionString);
            this.prefix = prefix;
            this.blobStates = details;
        }

        IEnumerable<BlobClient> GetAllBlobs(BlobContainerClient container, BlobTraits traits, string prefix)
        {
            foreach (BlobItem blob in container.GetBlobs(traits, this.blobStates, prefix))
            {
                yield return container.GetBlobClient(blob.Name);
            }
        }

        void DeleteAllBlobs(IEnumerable<BlobClient> items)
        {
            foreach (var blobItem in items)
            {
                blobItem.Delete(DeleteSnapshotsOption.IncludeSnapshots);

                /* if (blobItem.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory blob = (CloudBlobDirectory)blobItem;
                    DeleteAllBlobs(blob.ListBlobs(false, blobStates));
                }*/
            }
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteBlobsInContainerContainerNameIsNotAStringException();

            string containername = (string)(msg as IActorMessage).Message;
            var container = client.GetBlobContainerClient(containername);
            DeleteAllBlobs(this.GetAllBlobs(container, BlobTraits.All, prefix));
            return true;
        }
    }
}
