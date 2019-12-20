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
    public class RecieveFileInfo : FileActionBase, IActor
    {
        private string folderName;

        public RecieveFileInfo(string folderName)
        {
            this.folderName = folderName;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFilePath((msg.Message as string), folderName);
            msg.ProcessingResult = new System.IO.FileInfo(finalPath);
            return true;
        }
    }
}
