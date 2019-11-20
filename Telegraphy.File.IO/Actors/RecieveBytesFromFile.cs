using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.File.IO
{
    public class RecieveBytesFromBlobStorage : FileActionBase, IActor
    {
        private string folderName;

        public RecieveBytesFromBlobStorage(string folderName)
        {
            this.folderName = folderName;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotRecievedFileInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFinalPath((msg.Message as string), folderName);

            System.IO.File.Delete(finalPath);
            msg.ProcessingResult = System.IO.File.ReadAllBytes(finalPath);
            return true;
        }
    }
}
