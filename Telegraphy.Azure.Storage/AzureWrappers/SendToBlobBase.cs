using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
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
        private readonly bool overwrite = true;
        protected BlobContainerClient container;
        protected Func<string> blobNameFcn;
        protected Func<string, string> blobTransformNameFcn;
        protected Encoding encoding = Encoding.UTF8;

        public SendAndRecieveBlobBase(string storageConnectionString, string containerName, Func<string, string> blobNameFcn, bool overwrite = true)
        {
            var acct = new BlobServiceClient(storageConnectionString);
            container = acct.GetBlobContainerClient(containerName);
            this.blobTransformNameFcn = blobNameFcn;
            this.overwrite = overwrite;
        }

        protected SendAndRecieveBlobBase(string storageConnectionString, string containerName, Func<string> blobNameFcn, Encoding encoding, bool overwrite = true)
        {
            var acct = new BlobServiceClient(storageConnectionString);
            container = acct.GetBlobContainerClient(containerName);
            this.blobNameFcn = blobNameFcn;
            this.overwrite = overwrite;
            if (null != encoding)
                this.encoding = encoding;
            else
                this.encoding = Encoding.UTF8;
        }

        protected void SendString(BlockBlobClient blob, string message)
        {
            if (!overwrite && blob.Exists())
                return;
            
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(message)))
            {
                blob.Upload(ms, new BlobUploadOptions());
            }
        }

        protected void SendString(AppendBlobClient blob, string message)
        {
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(message)))
            {
                blob.AppendBlock(ms);
            }
        }

        protected void SendString(PageBlobClient blob, string message)
        {
            byte[] msgBytes = encoding.GetBytes(message);

            using (MemoryStream ms = new MemoryStream(msgBytes))
            {
                blob.UploadPages(ms,0);
            }
        }

        protected string RecieveString(BlobClient blob)
        {
            long size;
            byte[] msgBytes = RecieveBytes(blob, out size);
            return encoding.GetString(msgBytes);
        }
        
        protected void SendFile(BlockBlobClient blob,string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            if (!overwrite && blob.ExistsAsync().Result)
                return;

            using (StreamReader sr = new StreamReader(fileNameAndPath))
            {
                blob.Upload(sr.BaseStream, new BlobUploadOptions());
            }
        }

        protected void SendFile(AppendBlobClient blob, string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            using (StreamReader sr = new StreamReader(fileNameAndPath))
            {
                blob.AppendBlock(sr.BaseStream);
            }
        }

        protected void SendFile(PageBlobClient blob, string fileNameAndPath)
        {
            if (!File.Exists(fileNameAndPath))
                throw new CantSendFileDataWhenFileDoesNotExistException(fileNameAndPath);

            if (!overwrite && blob.Exists())
                return;

            using (StreamReader sr = new StreamReader(fileNameAndPath))
            {
                blob.UploadPages(sr.BaseStream,0);
            }
        }

        protected void RecieveFile(BlobClient blob, string fileNameAndPath, FileMode mode)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(fileNameAndPath, mode)))
            {
                blob.DownloadTo(sw.BaseStream);
            }
        }

        protected void SendStream(BlockBlobClient blob, Stream stream)
        {
            if (!overwrite && blob.Exists())
                return;

            blob.Upload(stream, new BlobUploadOptions());
        }

        protected void SendStream(AppendBlobClient blob, Stream stream)
        {
            blob.AppendBlock(stream);
        }

        protected void SendStream(PageBlobClient blob, Stream stream)
        {
            if (!overwrite && blob.Exists())
                return;

            blob.UploadPages(stream, 0);
        }

        protected void RecieveStream(BlobClient blob, Stream stream)
        {
            blob.DownloadTo(stream);
        }
        protected void SendBytes(BlobClient blob, byte[] msgBytes)
        {
            if (!overwrite && blob.Exists())
                return;

            using (MemoryStream ms = new MemoryStream(msgBytes))
            {
                blob.Upload(ms, new BlobUploadOptions());
            }
        }

        protected void SendBytes(BlockBlobClient blob, byte[] msgBytes)
        {
            if (!overwrite && blob.Exists())
                return;

            using (MemoryStream ms = new MemoryStream(msgBytes))
            {
                blob.Upload(ms, new BlobUploadOptions());
            }
        }

        protected void SendBytes(PageBlobClient blob, byte[] msgBytes)
        {
            if (!overwrite && blob.Exists())
                return;

            using (MemoryStream ms = new MemoryStream(msgBytes))
            {
                blob.UploadPages(ms, 0);
            }
        }

        protected void SendBytes(AppendBlobClient blob, byte[] msgBytes)
        {
            using (MemoryStream ms = new MemoryStream(msgBytes))
            {
                blob.AppendBlock(ms);
            }
        }

        protected byte[] RecieveBytes(BlobClient blob, out long size)
        {
            size = blob.GetProperties().Value.ContentLength;
            byte[] buffer = new byte[size];

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                blob.DownloadTo(ms);

                return ms.ToArray();
            }
        }
    }
}
