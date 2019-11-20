﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegraphy.File.IO.Exceptions;
using Telegraphy.Net;

namespace Telegraphy.File.IO
{
    public class RecieveFileAsStringArray : FileActionBase, IActor
    {
        private System.IO.FileMode mode;
        private string folderName;

        public RecieveFileAsStringArray(string folderName)
        {
            this.folderName = folderName;
        }

        bool IActor.OnMessageRecieved<T>(T msg)
        {
            if (!(msg as IActorMessage).Message.GetType().Name.Equals("String"))
                throw new CannotDeleteFilesInFolderFileNameIsNotAStringException();

            string finalPath = this.GetFinalPath((msg.Message as string), folderName);
            msg.ProcessingResult = System.IO.File.ReadAllLines(finalPath);
            return true;
        }
    }
}
