using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class RecieveFileAsStream : FileActionBase, IActor
    {
        private System.IO.FileMode mode;
        private string folderName;

        public RecieveFileAsStream(string folderName)
        {
            this.folderName = folderName;
            this.mode = System.IO.FileMode.OpenOrCreate;
        }

        public RecieveFileAsStream(string folderName, System.IO.FileMode mode)
        {
            this.folderName = folderName;
            this.mode = mode;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFilePath((msg.Message as string), folderName);
            msg.ProcessingResult = System.IO.File.Open(finalPath, this.mode);
            return true;
        }
    }
}
