using System;
using System.Collections.Generic;
using System.Text;

namespace Telegraphy.File.IO
{
    public class DirectoryQueue
    {
        public DirectoryQueue(string rootQueueDirectory, string queueName)
        {

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
            throw new NotImplementedException();
        }

        public void AddMessage(DirectoryQueueMessage msg, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
        {
            throw new NotImplementedException();
        }

        public void DeleteMessage(DirectoryQueueMessage msg)
        {
            throw new NotImplementedException();
        }

        public DirectoryQueueMessage GetMessage(TimeSpan? retrieveVisibilityTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
