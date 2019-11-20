using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.File.IO
{
    public class SendBytesToTruncateFile : FileActionBase, IActor
    {
        private string pathToFile;

        public SendBytesToTruncateFile(string pathToFile)
        {
            this.pathToFile = pathToFile;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("Byte[]"))
                throw new SendBytesCanOnlySendValueTypeByteArrayMessagesException("ValueTypeMessage<byte>");

            System.IO.FileMode mode = System.IO.File.Exists(this.pathToFile) ? System.IO.FileMode.Create : System.IO.FileMode.Truncate;

            byte[] msgBytes = (byte[])msg.Message;
            using (System.IO.FileStream fs = System.IO.File.Open(this.pathToFile, mode))
            {
                fs.Write(msgBytes, 0, msgBytes.Length);
            }
            return true;
        }
    }
}
