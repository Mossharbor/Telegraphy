using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using System.IO;
using Telegraphy.File.IO.Exceptions;

namespace Telegraphy.File.IO
{
    public class DeleteAllFilesInFolder : IActor
    {
        private string localFolder = null;
        private string wildcard = null;

        public DeleteAllFilesInFolder(string localFolder) :
            this(localFolder, null)
        {
        }

        public DeleteAllFilesInFolder(string localFolder, string wildCard = null)
        {
            this.localFolder = localFolder;
            this.wildcard = wildcard ?? "*";
        }

        void DeleteAllFiles (IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderNameIsNotAStringException();


            string folderName = (string)(msg as IActorMessage).Message;
            DeleteAllFiles(Directory.GetFiles(folderName, wildcard));

            return true;
        }
    }
}
