using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.File.IO
{
    public class SendBytesToLocalQueue : FileActionBase, IActor
    {
        private string queuePath;

        public SendBytesToLocalQueue(string queuePath)
        {
            this.queuePath = queuePath;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            string fileNameAndPath = this.GetFinalPath(this.queuePath, DateTime.Now.Ticks.ToString("00000000"));
            while (System.IO.File.Exists(fileNameAndPath))
            {
                fileNameAndPath = this.GetFinalPath(this.queuePath, DateTime.Now.Ticks.ToString("00000000"));
            }

            SendBytesToTruncateFile writer = new SendBytesToTruncateFile(fileNameAndPath);

            return writer.Tell(msg);
        }
    }
}
