using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;


namespace Telegraphy.File.IO
{
    public class SendStringToAppendToFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendStringToAppendToFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToFileException();

            string msgStr = (string)msg.Message;
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(this.pathToFile, true))
            {
                sw.Write(msgStr);
            }
            return true;
        }
    }
}
