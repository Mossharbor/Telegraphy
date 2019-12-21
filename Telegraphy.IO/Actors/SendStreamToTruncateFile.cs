using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class SendStreamToTruncateFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendStreamToTruncateFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        internal void Truncate(Stream msgStrM)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(this.pathToFile, false))
            {
                msgStrM.CopyTo(sw.BaseStream);
            }
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToFileException("Stream");

            Stream msgStr = (Stream)msg.Message;

            this.Truncate(msgStr);

            return true;
        }
    }
}
