using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Telegraphy.File.IO
{
    [Serializable]
    public class DirectoryQueueMessage
    {
        private string localPathToQueuedMessage = null;
        private bool deleted = false;

        public DirectoryQueueMessage()
        {
        }

        public DirectoryQueueMessage(string message)
        {
            this.SetMessageContent(message);
        }

        public DirectoryQueueMessage(byte[] messageContents)
        {
            this.SetMessageContent(messageContents);
        }

        internal void SetMessagePath(string messagePath)
        {
            this.LocalPathToQueuedMessage = messagePath;
        }

        internal void SetMessageContent(byte[] contents)
        {
            if (this.deleted)
                return;

            this.Message = contents;
        }

        public void SetMessageContent(string contents)
        {
            if (this.deleted)
                return;

            this.Message = System.Text.Encoding.UTF8.GetBytes(contents);
        }

        public void Delete()
        {
            this.deleted = true;
            System.IO.File.Delete(this.localPathToQueuedMessage);
        }

        public byte[] Message { get; set; }

        public int DequeueCount { get; set; }

        public string AsString
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(this.Message);
            }
        }

        internal void Hide(TimeSpan? visibilityTimeout)
        {
            System.IO.File.SetAttributes(this.localPathToQueuedMessage, System.IO.FileAttributes.Hidden);

            if (visibilityTimeout != null && visibilityTimeout.HasValue)
            {
                Task.Delay(visibilityTimeout.Value).ContinueWith((t) =>
                {
                    try
                    {
                        if (!this.deleted && t.IsCompleted && System.IO.File.Exists(this.localPathToQueuedMessage))
                        {
                            System.IO.File.SetAttributes(this.localPathToQueuedMessage, System.IO.FileAttributes.Normal);
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        public byte[] AsBytes
        {
            get
            {
                return this.Message;
            }
        }

        internal string LocalPathToQueuedMessage { get => localPathToQueuedMessage; set => localPathToQueuedMessage = value; }

        internal void SetInitialVisibilityDelay(TimeSpan? initialVisibilityDelay)
        {
            this.InitialVisibilityDelay = initialVisibilityDelay;

        }

        internal void SetTimeToLive(TimeSpan? timeToLive)
        {
            this.TimeToLive = timeToLive;

            if (null == timeToLive || !timeToLive.HasValue)
                return;

            Task.Delay(this.TimeToLive.Value).ContinueWith((t) =>
            {
                try
                {
                    if (!this.deleted && t.IsCompleted && System.IO.File.Exists(this.localPathToQueuedMessage))
                    {
                        this.Delete();
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        public TimeSpan? TimeToLive { get; internal set; }
        public TimeSpan? InitialVisibilityDelay { get; internal set; }

        internal void Store()
        {
            if (null != InitialVisibilityDelay && !System.IO.File.Exists(this.localPathToQueuedMessage))
            {
                string tempPath = System.IO.Path.GetTempPath();
                string tempName = System.IO.Path.GetFileName(this.localPathToQueuedMessage);
                string tempFile = System.IO.Path.Combine(tempPath, tempName);
                System.IO.File.Create(tempFile).Close();
                System.IO.File.SetAttributes(tempFile, System.IO.FileAttributes.Hidden);
                System.IO.File.Move(tempFile, this.localPathToQueuedMessage);
                this.Hide(this.InitialVisibilityDelay);
            }

            IFormatter formatter = new BinaryFormatter();
            byte[] messageInBytes = null;
            using (var ms = new System.IO.MemoryStream())
            {
                formatter.Serialize(ms, this);
                messageInBytes = ms.GetBuffer();
            }

            SendBytesToTruncateFile truncator = new SendBytesToTruncateFile(this.localPathToQueuedMessage);
            if (!this.deleted)
            {
                truncator.Truncate(messageInBytes);
            }
        }

        internal static DirectoryQueueMessage FromQueueFile(string msgpath)
        {
            IFormatter formatter = new BinaryFormatter();
            using (var stream = System.IO.File.OpenRead(msgpath))
            {
                return (DirectoryQueueMessage)formatter.Deserialize(stream);
            }
        }
    }
}
