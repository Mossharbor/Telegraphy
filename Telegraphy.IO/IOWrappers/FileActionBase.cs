using System;
using System.Collections.Generic;
using System.Text;
using Telegraphy.IO.Exceptions;

namespace Telegraphy.IO
{
    public class FileActionBase
    {
        protected string GetFolderPath(string folderName, string subFolder)
        {
            return System.IO.Path.Combine(folderName, subFolder);
        }

        protected string GetFilePath(string fileName, string folderName, bool expectFile = true)
        {
            string finalPath = fileName;
            if (!System.IO.File.Exists(fileName) && !string.IsNullOrWhiteSpace(folderName))
            {
                finalPath = System.IO.Path.Combine(folderName, fileName);
            }

            if (System.IO.File.Exists(fileName) && !string.IsNullOrWhiteSpace(folderName))
            {
                if (!fileName.ToLower().StartsWith(folderName.ToLower()))
                {
                    throw new CannotDeleteFileAsItDoesNotResideUnderFolderException($"{fileName} is not under folder {folderName}");
                }
            }
            else if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new NullReferenceException($"Filename cannot be null. '{fileName}'");
            }
            else if (expectFile && !System.IO.File.Exists(finalPath))
            {
                throw new System.IO.FileNotFoundException($"Could not find the path to {finalPath}'");
            }

            return finalPath;
        }
    }
}
