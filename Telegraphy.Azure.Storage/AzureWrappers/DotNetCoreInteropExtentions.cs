using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.Azure
{
    public static class DotNetCoreInteropExtentions
    {
#if NETSTANDARD2_0
        public static TableResult Execute(this CloudTable table, TableOperation op)
        {
            return table.ExecuteAsync(op).Result;
        }

        public static void FetchAttributes(this CloudBlob blob)
        {
            blob.FetchAttributesAsync().Wait();
        }

        public static int DownloadToByteArray(this CloudBlob blob, byte[] buffer, int index)
        {
            return blob.DownloadToByteArrayAsync(buffer, index).Result;
        }

        public static int DownloadToByteArray(this CloudBlockBlob blob, byte[] buffer, int index)
        {
            return blob.DownloadToByteArrayAsync(buffer, index).Result;
        }

        public static int DownloadToByteArray(this CloudAppendBlob blob, byte[] buffer, int index)
        {
            return blob.DownloadToByteArrayAsync(buffer, index).Result;
        }

        public static int DownloadToByteArray(this CloudPageBlob blob, byte[] buffer, int index)
        {
            return blob.DownloadToByteArrayAsync(buffer, index).Result;
        }

        public static void DownloadToStream(this CloudBlob blob, System.IO.Stream stream)
        {
            blob.DownloadToStreamAsync(stream).Wait();
        }

        public static void DownloadToStream(this CloudAppendBlob blob, System.IO.Stream stream)
        {
            blob.DownloadToStreamAsync(stream).Wait();
        }

        public static void DownloadToStream(this CloudPageBlob blob, System.IO.Stream stream)
        {
            blob.DownloadToStreamAsync(stream).Wait();
        }

        public static void DownloadToStream(this CloudBlockBlob blob, System.IO.Stream stream)
        {
            blob.DownloadToStreamAsync(stream).Wait();
        }

        public static void UploadFromStream(this CloudAppendBlob blob, System.IO.Stream stream)
        {
            blob.UploadFromStreamAsync(stream).Wait();
        }

        public static void UploadFromStream(this CloudPageBlob blob, System.IO.Stream stream)
        {
            blob.UploadFromStreamAsync(stream).Wait();
        }

        public static void UploadFromStream(this CloudBlockBlob blob, System.IO.Stream stream)
        {
            blob.UploadFromStreamAsync(stream).Wait();
        }

        public static void AppendFromByteArray(this CloudAppendBlob blob, byte[] buffer, int offset, int count)
        {
            blob.AppendFromByteArrayAsync(buffer, offset, count).Wait();
        }

        public static void AppendFromStream(this CloudAppendBlob blob, System.IO.Stream stream)
        {
            blob.AppendFromStreamAsync(stream).Wait();
        }

        public static void AppendFromFile(this CloudAppendBlob blob, string path)
        {
            blob.AppendFromFileAsync(path).Wait();
        }

        public static void AppendText(this CloudAppendBlob blob, string text)
        {
            blob.AppendTextAsync(text).Wait();
        }

        public static void DownloadToFile(this CloudBlob blob, string path, System.IO.FileMode mode)
        {
            blob.DownloadToFileAsync(path, mode).Wait();
        }

        public static void DownloadToFile(this CloudPageBlob blob, string path, System.IO.FileMode mode)
        {
            blob.DownloadToFileAsync(path, mode).Wait();
        }

        public static void DownloadToFile(this CloudBlockBlob blob, string path, System.IO.FileMode mode)
        {
            blob.DownloadToFileAsync(path, mode).Wait();
        }

        public static void DownloadToFile(this CloudAppendBlob blob, string path, System.IO.FileMode mode)
        {
            blob.DownloadToFileAsync(path, mode).Wait();
        }

        public static void UploadFromFile(this CloudPageBlob blob, string path)
        {
            blob.UploadFromFileAsync(path).Wait();
        }

        public static void UploadFromFile(this CloudBlockBlob blob, string path)
        {
            blob.UploadFromFileAsync(path).Wait();
        }

        public static void UploadFromFile(this CloudAppendBlob blob, string path)
        {
            blob.UploadFromFileAsync(path).Wait();
        }

        public static void UploadFromByteArray(this CloudPageBlob blob, byte[] buffer, int index, int count)
        {
            blob.UploadFromByteArrayAsync(buffer, index, count).Wait();
        }

        public static void UploadFromByteArray(this CloudBlockBlob blob, byte[] buffer, int index, int count)
        {
            blob.UploadFromByteArrayAsync(buffer, index, count).Wait();
        }

        public static void UploadFromByteArray(this CloudAppendBlob blob, byte[] buffer, int index, int count)
        {
            blob.UploadFromByteArrayAsync(buffer, index, count).Wait();
        }

        public static void UploadText(this CloudPageBlob blob, string text, Encoding encoding = null)
        {
            byte[] msgBytes = null != encoding ? encoding.GetBytes(text) : Encoding.UTF8.GetBytes(text);
            blob.UploadFromByteArrayAsync(msgBytes,0, msgBytes.Length).Wait();
        }

        public static void UploadText(this CloudBlockBlob blob, string text, Encoding encoding = null)
        {
            byte[] msgBytes = null != encoding ? encoding.GetBytes(text) : Encoding.UTF8.GetBytes(text);
            blob.UploadFromByteArrayAsync(msgBytes, 0, msgBytes.Length).Wait();
        }

        public static void UploadText(this CloudAppendBlob blob, string text, Encoding encoding = null)
        {
            byte[] msgBytes = null != encoding ? encoding.GetBytes(text) : Encoding.UTF8.GetBytes(text);
            blob.UploadFromByteArrayAsync(msgBytes, 0, msgBytes.Length).Wait();
        }

        public static void CreateIfNotExists(this CloudAppendBlob blob)
        {
            if (blob.ExistsAsync().Result)
                blob.CreateOrReplaceAsync().Wait();
        }

        public static bool Exists(this CloudPageBlob blob)
        {
            return blob.ExistsAsync().Result;
        }

        public static bool Exists(this CloudBlockBlob blob)
        {
            return blob.ExistsAsync().Result;
        }

        public static bool Exists(this CloudAppendBlob blob)
        {
            return blob.ExistsAsync().Result;
        }

        public static void CreateOrReplace(this CloudAppendBlob blob)
        {
            blob.CreateOrReplaceAsync();
        }

        public static void Delete(this CloudBlob blob)
        {
            blob.DeleteAsync().Wait();
        }

        public static void Delete(this CloudBlockBlob blob)
        {
            blob.DeleteAsync().Wait();
        }

        public static void Delete(this CloudAppendBlob blob)
        {
            blob.DeleteAsync().Wait();
        }

        public static void Delete(this CloudPageBlob blob)
        {
            blob.DeleteAsync().Wait();
        }

        public static void Delete(this CloudBlobContainer container)
        {
            container.DeleteAsync().Wait();
        }

        public static IEnumerable<IListBlobItem> ListBlobs(this CloudBlobDirectory blob, bool useFlatBlobListing = false, BlobListingDetails blobListingDetails = BlobListingDetails.None, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            BlobContinuationToken token = new BlobContinuationToken();
            BlobResultSegment segment = blob.ListBlobsSegmentedAsync(useFlatBlobListing, blobListingDetails, int.MaxValue, token, options, operationContext).Result;
            while (segment.ContinuationToken != null)
            {
                foreach (var t in segment.Results)
                    yield return t;

                segment = blob.ListBlobsSegmentedAsync(useFlatBlobListing, blobListingDetails, int.MaxValue, token, options, operationContext).Result;
            }
        }

        public static IEnumerable<IListBlobItem> ListBlobs(this CloudBlobContainer container, bool useFlatBlobListing = false, BlobListingDetails blobListingDetails = BlobListingDetails.None, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            return container.ListBlobs(String.Empty, useFlatBlobListing, blobListingDetails, options, operationContext);
        }

        public static IEnumerable<IListBlobItem> ListBlobs(this CloudBlobContainer container,string prefix, bool useFlatBlobListing = false, BlobListingDetails blobListingDetails = BlobListingDetails.None, BlobRequestOptions options = null, OperationContext operationContext = null)
        {
            BlobContinuationToken token = new BlobContinuationToken();
            BlobResultSegment segment = container.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, int.MaxValue, token, options, operationContext).Result;
            while (segment.ContinuationToken != null)
            {
                foreach (var t in segment.Results)
                    yield return t;

                segment = container.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, blobListingDetails, int.MaxValue, token, options, operationContext).Result;
            }
        }

        public static void FetchAttributes(this CloudQueue queue)
        {
            queue.FetchAttributesAsync().Wait();
        }

        public static bool CreateIfNotExists(this CloudQueue queue)
        {
            return queue.CreateIfNotExistsAsync().Result;
        }

        public static void DeleteMessage(this CloudQueue queue, CloudQueueMessage message)
        {
            queue.DeleteMessageAsync(message).Wait();
        }

        public static void DeleteMessage(this CloudQueue queue, string messageId, string popReceipt, QueueRequestOptions options)
        {
            queue.DeleteMessageAsync(messageId, popReceipt, options, null).Wait();
        }

        public static void DeleteMessage(this CloudQueue queue, string messageId, string popReceipt, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.DeleteMessageAsync(messageId, popReceipt, options, operationContext).Wait();
        }

        public static void DeleteMessage(this CloudQueue queue, string messageId, string popReceipt)
        {
            queue.DeleteMessageAsync(messageId, popReceipt).Wait();
        }

        public static void DeleteMessage(this CloudQueue queue, CloudQueueMessage message, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.DeleteMessageAsync(message, options, operationContext).Wait();
        }

        public static CloudQueueMessage GetMessage(this CloudQueue queue)
        {
            return queue.GetMessageAsync().Result;
        }

        public static CloudQueueMessage GetMessage(this CloudQueue queue, TimeSpan? visibilityTimeout, QueueRequestOptions options, OperationContext operationContext)
        {
            return queue.GetMessageAsync(visibilityTimeout, options, operationContext).Result;
        }
        
        public static IEnumerable<CloudQueueMessage> GetMessages(this CloudQueue queue, int messageCount, TimeSpan? visibilityTimeout, QueueRequestOptions options)
        {
            return queue.GetMessagesAsync(messageCount, visibilityTimeout, options, null).Result;
        }

        public static IEnumerable<CloudQueueMessage> GetMessages(this CloudQueue queue,int messageCount, TimeSpan? visibilityTimeout, QueueRequestOptions options, OperationContext operationContext)
        {
            return queue.GetMessagesAsync(messageCount, visibilityTimeout, options, operationContext).Result;
        }

        public static IEnumerable<CloudQueueMessage> GetMessages(this CloudQueue queue,int messageCount)
        {
            return queue.GetMessagesAsync(messageCount).Result;
        }
        
        public static bool Exists(this CloudQueue queue)
        {
            return queue.ExistsAsync().Result;
        }

        public static void AddMessage(this CloudQueue queue, CloudQueueMessage message)
        {
            queue.AddMessageAsync(message).Wait();
        }

        public static void AddMessage(this CloudQueue queue, CloudQueueMessage message, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.AddMessageAsync(message, timeToLive, initialVisibilityDelay, options, operationContext).Wait();
        }
        
        public static void Clear(this CloudQueue queue, QueueRequestOptions options, OperationContext operationContext)
        {
            queue.ClearAsync(options, operationContext).Wait();

        }
        public static void Clear(this CloudQueue queue)
        {
            queue.ClearAsync().Wait();
        }
#endif
    }
}
