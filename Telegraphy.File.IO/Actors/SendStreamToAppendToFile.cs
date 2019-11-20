using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;


namespace Telegraphy.File.IO
{
    public class SendStreamToAppendToFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendStreamToAppendToFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!((msg as IActorMessage).Message is Stream))
                throw new CannotSendNonStreamMessagesToFileException("ValueTypeMessage<byte>");

            Stream msgStream = (Stream)msg.Message;
            using (System.IO.FileStream fs = System.IO.File.Open(this.pathToFile, System.IO.FileMode.OpenOrCreate))
            {
                fs.Position = fs.Length;
                msgStream.CopyTo(fs);
            }
            return true;
        }
    }
}
