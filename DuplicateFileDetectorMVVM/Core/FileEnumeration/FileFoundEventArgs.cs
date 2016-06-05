using System;
using System.IO;


namespace DuplicateFileDetectorMVVM.IO
{
    public class FileFoundEventArgs : EventArgs
    {
        public FileInfo fileInfo { get; set; }
        public object UserContext { get; set; }

        public FileFoundEventArgs(FileInfo inFileInfo, object inUserContext)
        {
            fileInfo = inFileInfo;
            UserContext = inUserContext;
        }
    }
}
