using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Telegraphy.File.IO
{
    [Serializable]
    public class DirectoryQueueMessage
    {
        private string messagePath = null;
        private bool deleted = false;

        public DirectoryQueueMessage()
        {
        }

        internal DirectoryQueueMessage(string messagePath)
        {
            this.MessagePath = messagePath;
        }

        internal void SetMessageContent(byte[] contents)
        {
            if (this.deleted)
                return;

            this.Message = contents;
            this.Store();
        }

        public void SetMessageContent(string contents)
        {
            if (this.deleted)
                return;

            this.Message = System.Text.Encoding.UTF8.GetBytes(contents);
            this.Store();
        }

        public void Delete()
        {
            this.deleted = true;
            System.IO.File.Delete(this.messagePath);
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

        internal void Hide(TimeSpan? retrieveVisibilityTimeout)
        {
            System.IO.File.SetAttributes(this.messagePath, System.IO.FileAttributes.Hidden);
            // TODO set reteval visibility timer in message;
            throw new NotImplementedException();
        }

        public byte[] AsBytes
        {
            get
            {
                return this.Message;
            }
        }

        internal string MessagePath { get => messagePath; set => messagePath = value; }

        internal void Store()
        {
            IFormatter formatter = new BinaryFormatter();
            byte[] messageInBytes = null;
            using (var ms = new System.IO.MemoryStream())
            {
                formatter.Serialize(ms, this);
                messageInBytes = ms.GetBuffer();
            }

            SendBytesToTruncateFile truncator = new SendBytesToTruncateFile(this.messagePath);
            if (!this.deleted)
            {
                truncator.Truncate(messageInBytes);
            }
        }

        internal static DirectoryQueueMessage FromQueueFile(string msgpath)
        {
            IFormatter formatter = new BinaryFormatter();
            return (DirectoryQueueMessage)formatter.Deserialize(System.IO.File.OpenRead(msgpath));
        }
    }
}
