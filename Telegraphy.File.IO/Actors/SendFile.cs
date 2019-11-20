using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.File.IO
{
    public class SendFile : FileActionBase, IActor
    {
        private string folderName;

        public SendFile()
        {
        }

        public SendFile(string folderName)
        {
            this.folderName = folderName;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToFileException();

            string sourcePath = (string)msg.Message;
            string fileName = Path.GetFileName((string)msg.Message);
            string finalpath = this.GetFinalPath(fileName, this.folderName);

            System.IO.File.Copy(sourcePath, finalpath);

            return true;
        }
    }
}
