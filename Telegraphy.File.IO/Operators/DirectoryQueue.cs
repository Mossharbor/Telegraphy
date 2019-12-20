using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Telegraphy.File.IO.Exceptions;

namespace Telegraphy.File.IO
{
    public class DirectoryQueue : IDisposable
    {
        bool acquiredLock = false;
        TimeSpan timeToWaitForLock = TimeSpan.FromSeconds(3);
        string rootQueueDirectory;
        string queueName;
        string queuePath;
        Semaphore queueLock = null;
        private const string msgExtension = ".qitem";

        public DirectoryQueue(string rootQueueDirectory, string queueName)
        {
            this.rootQueueDirectory = rootQueueDirectory;
            this.queueName = queueName;
            this.queuePath = System.IO.Path.Combine(this.rootQueueDirectory, this.queueName);

            this.queueLock = new Semaphore(1, 1, this.queueName);
        }

        private void AcquireLock()
        {
            if (this.queueLock.WaitOne(this.timeToWaitForLock))
            {
                System.Diagnostics.Debug.WriteLine("Acquired Lock");
                this.acquiredLock = true;
                return;
            }

            throw new CouldNotAcquireLockOnQueueInTimeAllottedException(this.timeToWaitForLock.TotalMilliseconds.ToString() + " ms.");
        }

        private void ReleaseLock()
        {
            if (!this.acquiredLock)
                return;

            System.Diagnostics.Debug.WriteLine("Released Lock");
            this.queueLock.Release();
            this.acquiredLock = false;
        }

        public void CreateIfNotExists()
        {
            if (System.IO.Directory.Exists(queuePath))
                return;

            try
            {
                this.AcquireLock();
                System.Diagnostics.Debug.WriteLine("CreateIfNotExists");
                if (!System.IO.Directory.Exists(queuePath))
                {
                    System.IO.Directory.CreateDirectory(queuePath);
                }
            }
            finally
            {
                this.ReleaseLock();
            }
        }

        public void FetchAttributes()
        {
            checked
            {
                ulong msgCount = 0;

                try
                {
                    System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(queuePath);
                    foreach (System.IO.FileInfo fi in dInfo.EnumerateFiles($"*{msgExtension}"))
                    {
                        if ((fi.Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
                        {
                            ++msgCount;
                        }
                    }
                }
                catch(Exception)
                {
                }
                finally
                {
                    this.approximateMessageCount = msgCount;
                }
            }
        }

        public void Delete()
        {
            try
            {
                this.AcquireLock();
                System.Diagnostics.Debug.WriteLine("Delete");
                if (System.IO.Directory.Exists(queuePath))
                {
                    System.IO.Directory.Delete(queuePath, true);
                }
            }
            finally
            {
                this.ReleaseLock();
                this.queueLock.Close();
            }
        }

        private ulong approximateMessageCount;
        public ulong ApproximateMessageCount
        {
            get
            {
                return approximateMessageCount;
            }
        }

        public void AddMessage(DirectoryQueueMessage msg)
        {
            this.AddMessage(msg, null, null);
        }

        public void AddMessage(DirectoryQueueMessage msg, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
        {
            try
            {
                this.AcquireLock();
                System.Diagnostics.Debug.WriteLine("Add Message");

                // set the msg name;
                string fileNameAndPath = System.IO.Path.Combine(this.queuePath, DateTime.Now.Ticks.ToString("0000000000") + msgExtension);
                while (System.IO.File.Exists(fileNameAndPath))
                {
                    fileNameAndPath = System.IO.Path.Combine(this.queuePath, DateTime.Now.Ticks.ToString("0000000000") + msgExtension);
                }

                msg.SetTimeToLive(timeToLive);
                msg.SetInitialVisibilityDelay(initialVisibilityDelay);
                msg.LocalPathToQueuedMessage = fileNameAndPath;
                msg.Store();
            }
            finally
            {
                this.ReleaseLock();
            }
        }

        public void DeleteMessage(DirectoryQueueMessage msg)
        {
            try
            {
                this.AcquireLock();
                System.Diagnostics.Debug.WriteLine("Delete Message");
                msg.Delete();
            }
            finally
            {
                this.ReleaseLock();
            }
        }

        public DirectoryQueueMessage GetMessage(TimeSpan? retrieveVisibilityTimeout = null)
        {
            if (!System.IO.Directory.Exists(this.queuePath))
                return null;

            try
            {
                this.AcquireLock();
                System.Diagnostics.Debug.WriteLine("Get Message");
                string msgpath = null;
                System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(this.queuePath);
                foreach (var t in dInfo.GetFileSystemInfos($"*{msgExtension}").OrderBy(p=>p.CreationTime))
                {
                    if ((t.Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden)
                        msgpath = t.FullName;
                }

                if (null == msgpath)
                    return null;

                DirectoryQueueMessage nextMessage = DirectoryQueueMessage.FromQueueFile(msgpath);

                System.Diagnostics.Debug.WriteLine(msgpath);

                // NOTE: we release the lock after we have the message because we update the visibility for the file
                nextMessage.DequeueCount++;
                nextMessage.Hide(retrieveVisibilityTimeout);
                nextMessage.Store(); // make sure dequeue count has been stored correctly.

                return nextMessage;
            }
            finally
            {
                this.ReleaseLock();
            }
        }

        public void Dispose()
        {
            try
            {
                if (acquiredLock)
                {
                    System.Diagnostics.Debug.WriteLine("Disposed");
                    this.ReleaseLock();
                }
            }
            catch(Exception)
            {
            }
        }
    }
}
