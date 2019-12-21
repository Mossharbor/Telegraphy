using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using System.IO;
using Telegraphy.IO.Exceptions;

namespace Telegraphy.IO
{
    public class DeleteAllFilesInFolder : IActor
    {
        private string wildcard = null;

        public DeleteAllFilesInFolder() :
            this(null)
        {
        }

        public DeleteAllFilesInFolder(string wildCard = null)
        {
            this.wildcard = wildcard ?? "*";
        }

        internal void DeleteAllFiles (IEnumerable<string> files)
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
