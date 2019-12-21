using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class SendStringToTruncateFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendStringToTruncateFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        internal void Truncate(string msgStr)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(this.pathToFile, false))
            {
                sw.Write(msgStr);
            }
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToFileException();

            string msgStr = (string)msg.Message;

            this.Truncate(msgStr);

            return true;
        }
    }
}
