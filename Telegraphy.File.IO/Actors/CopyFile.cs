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
    public class CopyFile : FileActionBase, IActor
    {
        private string folderName;
        private bool overwrite = false;

        public CopyFile()
        {
        }

        public CopyFile(string folderName, bool overwrite = false)
        {
            this.folderName = folderName;
            this.overwrite = overwrite;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotSendNonStringMessagesToFileException();

            string sourcePath = (string)msg.Message;
            string fileName = Path.GetFileName((string)msg.Message);
            string finalpath = this.GetFilePath(fileName, this.folderName, false);

            System.IO.File.Copy(sourcePath, finalpath, overwrite);

            return true;
        }
    }
}
