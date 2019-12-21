using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;


namespace Telegraphy.IO
{
    public class SendBytesToAppendToFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendBytesToAppendToFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                throw new SendBytesCanOnlySendValueTypeByteArrayMessagesException("ValueTypeMessage<byte>");

            byte[] msgBytes = (byte[])msg.Message;
            using (System.IO.FileStream fs = System.IO.File.Open(this.pathToFile, System.IO.FileMode.OpenOrCreate))
            {
                fs.Position = fs.Length;
                fs.Write(msgBytes, 0, msgBytes.Length);
            }
            return true;
        }
    }
}
