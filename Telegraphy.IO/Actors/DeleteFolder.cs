using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.IO.Exceptions;

namespace Telegraphy.IO
{
    public class DeleteFolder : FileActionBase, IActor
    {
        private string folderName;
        private DeleteAllFilesInFolder fileCleaner = new DeleteAllFilesInFolder();

        public DeleteFolder(string folderName)
        {
            this.folderName = folderName;
        }

        public DeleteFolder()
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderFileNameIsNotAStringException();

            string finalPath = !string.IsNullOrEmpty(folderName) ? this.GetFolderPath(folderName, (string)(msg as IActorMessage).Message) : (string)(msg as IActorMessage).Message;

            fileCleaner.DeleteAllFiles(System.IO.Directory.GetFiles(finalPath));

            System.IO.Directory.Delete(finalPath);

            return true;
        }
    }
}
