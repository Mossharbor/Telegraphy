using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO
{
    public class DirectoryQueue
    {
        string rootQueueDirectory;
        string queueName;

        public DirectoryQueue(string rootQueueDirectory, string queueName)
        {
            this.rootQueueDirectory = rootQueueDirectory;
            this.queueName = queueName;
        }

        public void CreateIfNotExists()
        {
            throw new NotImplementedException();
        }

        public void FetchAttributes()
        {
            throw new NotImplementedException();
        }

        public ulong ApproximateMessageCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void AddMessage(DirectoryQueueMessage msg)
        {
            this.AddMessage(msg, null, null);
        }

        public void AddMessage(DirectoryQueueMessage msg, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
        {
            // set the msg name;
            string fileNameAndPath = System.IO.Path.Combine(this.rootQueueDirectory, this.queueName, DateTime.Now.Ticks.ToString("00000000"));
            while (System.IO.File.Exists(fileNameAndPath))
            {
                fileNameAndPath = System.IO.Path.Combine(this.rootQueueDirectory, this.queueName, DateTime.Now.Ticks.ToString("00000000"));
            }

            msg.MessagePath = fileNameAndPath;
            msg.Store();
        }

        public void DeleteMessage(DirectoryQueueMessage msg)
        {
            msg.Delete();
        }

        public DirectoryQueueMessage GetMessage(TimeSpan? retrieveVisibilityTimeout)
        {
            string[] itemsInQueue = System.IO.Directory.GetFiles(System.IO.Path.Combine(this.rootQueueDirectory, this.queueName, "*"));
            string msgpath = itemsInQueue[0];
            DirectoryQueueMessage nextMessage = DirectoryQueueMessage.FromQueueFile(msgpath);

            nextMessage.DequeueCount++;
            nextMessage.Hide(retrieveVisibilityTimeout);
            nextMessage.Store(); // make sure dequeue count has been stored correctly.

            return nextMessage;


        }
    }
}
