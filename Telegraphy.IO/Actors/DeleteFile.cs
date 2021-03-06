﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.Net;
using Telegraphy.IO.Exceptions;

namespace Telegraphy.IO
{
    public class DeleteFile : FileActionBase, IActor
    {
        private string folderName;

        public DeleteFile(string folderName)
        {
            this.folderName = folderName;
        }

        public DeleteFile()
        {
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFilePath((msg.Message as string), folderName);

            System.IO.File.Delete(finalPath);

            return true;
        }
    }
}
