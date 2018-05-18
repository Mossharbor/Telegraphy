﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Azure.Exceptions;
using Telegraphy.Net.Exceptions;

namespace Telegraphy.Azure
{
    public class SendAndRecieveBlobBase
    {
        protected CloudBlobContainer container;
        protected Func<string> blobNameFcn;
        protected Func<string, string> blobTransformNameFcn;
        protected Encoding encoding = Encoding.UTF8;

        public SendAndRecieveBlobBase(string storageConnectionString, string containerName, Func<string, string> blobNameFcn)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            container = client.GetContainerReference(containerName);
            this.blobTransformNameFcn = blobNameFcn;
        }

        protected SendAndRecieveBlobBase(string storageConnectionString, string containerName, Func<string> blobNameFcn, Encoding encoding)
        {
            var acct = CloudStorageAccount.Parse(storageConnectionString);
            var client = acct.CreateCloudBlobClient();
            container = client.GetContainerReference(containerName);
            this.blobNameFcn = blobNameFcn;
            if (null != encoding)
                this.encoding = encoding;
        }

        protected void SendString(CloudBlockBlob blob, string message)
        {
            blob.UploadText(message, encoding);
        }

        protected void SendString(CloudAppendBlob blob, string message)
        {
            blob.AppendText(message);
        }

        protected void SendString(CloudPageBlob blob, string message)
        {
            byte[] msgBytes = encoding.GetBytes(message);
            blob.UploadFromByteArray(msgBytes, 0, msgBytes.Length);
        }

        protected string RecieveString(CloudBlob blob)
        {
            int size;
            byte[] msgBytes = RecieveBytes(blob, out size);
            return encoding.GetString(msgBytes);
        }
        
        protected void SendFile(CloudBlockBlob blob,string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            blob.UploadFromFile(fileNameAndPath);
        }

        protected void SendFile(CloudAppendBlob blob, string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            blob.AppendFromFile(fileNameAndPath);
        }

        protected void SendFile(CloudPageBlob blob, string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            blob.UploadFromFile(fileNameAndPath);
        }

        protected void RecieveFile(CloudBlob blob, string fileNameAndPath, FileMode mode)
        {
            blob.DownloadToFile(fileNameAndPath,  mode);
        }

        protected void SendStream(CloudBlockBlob blob, Stream stream)
        {
            blob.UploadFromStream(stream);
        }

        protected void SendStream(CloudAppendBlob blob, Stream stream)
        {
            blob.AppendFromStream(stream);
        }

        protected void SendStream(CloudPageBlob blob, Stream stream)
        {
            blob.UploadFromStream(stream);
        }

        protected void RecieveStream(CloudBlob blob, Stream stream)
        {
            blob.DownloadToStream(stream);
        }
        
        protected void SendBytes(CloudBlockBlob blob, byte[] msgBytes)
        {
            blob.UploadFromByteArray(msgBytes, 0, msgBytes.Length);
        }

        protected void SendBytes(CloudPageBlob blob, byte[] msgBytes)
        {
            blob.UploadFromByteArray(msgBytes, 0, msgBytes.Length);
        }

        protected void SendBytes(CloudAppendBlob blob, byte[] msgBytes)
        {
            blob.AppendFromByteArray(msgBytes, 0, msgBytes.Length);
        }

        protected byte[] RecieveBytes(CloudBlob blob, out int size)
        {
            blob.FetchAttributes();
            byte[] msgBytes = new byte[blob.Properties.Length];
            size = blob.DownloadToByteArray(msgBytes, 0);
            return msgBytes;
        }
    }
}