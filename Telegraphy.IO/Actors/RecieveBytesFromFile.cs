using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.IO
{
    public class RecieveBytesFromFile : FileActionBase, IActor
    {
        private string folderName;

        public RecieveBytesFromFile(string folderName)
        {
            this.folderName = folderName;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotRecievedFileInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFilePath((msg.Message as string), folderName);

            msg.ProcessingResult = System.IO.File.ReadAllBytes(finalPath);
            return true;
        }
    }
}
