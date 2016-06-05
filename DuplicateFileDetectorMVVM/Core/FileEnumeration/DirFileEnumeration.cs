using System;
using System.Collections.Generic;
using System.IO;


namespace DuplicateFileDetectorMVVM.IO
{
    // A delegate type for hooking up change notifications.
    public delegate void FileFoundEventHandler(object sender, FileFoundEventArgs e);
    public delegate void DirectoryFoundEventHandler(object sender, DirectoryFoundEventArgs e);


    public class DirFileEnumeration
    {
        public event DirectoryFoundEventHandler DirectoryFound;
        public event FileFoundEventHandler FileFound;

        public object UserContext = null;

        public DirFileEnumeration(object inUserContext)
        {
            UserContext = inUserContext;
        }


        bool _stopEnumeration = false;

        protected virtual void OnFileFound(FileFoundEventArgs e)
        {
            if(FileFound != null)
                FileFound(this, e);
        }

        protected virtual void OnDirectoryFound(DirectoryFoundEventArgs e)
        {
            if(DirectoryFound != null)
                DirectoryFound(this, e);
        }

        public void StopEnumeration()
        {
            _stopEnumeration = true;
        }

        public void EnumerateFiles(string rootDirectory)
        {
            Stack<DirectoryInfo> DirectoryStack = new Stack<DirectoryInfo>();

            if(rootDirectory == null || rootDirectory.Length == 0)
            {
                return;
            }
            _stopEnumeration = false;

            DirectoryInfo rootDir = new DirectoryInfo(rootDirectory);
            DirectoryStack.Push(rootDir);

            while(DirectoryStack.Count != 0)
            {
                if(_stopEnumeration)
                    break;

                DirectoryInfo currentDirectory = DirectoryStack.Pop();
                try
                {
                    foreach(DirectoryInfo DirItem in currentDirectory.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        if(_stopEnumeration)
                            break;

                        try
                        {
                            DirectoryStack.Push(DirItem);
                            OnDirectoryFound(new DirectoryFoundEventArgs(DirItem));
                        }
                        catch(Exception Ex)
                        {
                            Console.WriteLine("Exception: {0}", Ex.Message);
                        }
                    }

                    // enum all files
                    try
                    {
                        foreach(FileInfo fInfo in currentDirectory.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly))
                        {
                            if(_stopEnumeration)
                                break;

                            try
                            {
                                OnFileFound(new FileFoundEventArgs(fInfo, UserContext));
                            }
                            catch(Exception)
                            {
                                Console.WriteLine("Exception: {0}", fInfo.Name);
                            }
                        }
                    }
                    catch(Exception Ex)
                    {
                        Console.WriteLine("Exception: {0}", Ex.Message);
                    }
                }
                catch(Exception Ex)
                {
                    Console.WriteLine("Exception: {0}", Ex.Message);
                }
            }
        }

        public void EnumerateDirectories(string rootDirectory)
        {
            Stack<DirectoryInfo> DirectoryStack = new Stack<DirectoryInfo>();

            if(rootDirectory == null || rootDirectory.Length == 0)
            {
                return;
            }

            _stopEnumeration = false;
            DirectoryInfo rootDir = new DirectoryInfo(rootDirectory);
            DirectoryStack.Push(rootDir);

            while(DirectoryStack.Count != 0)
            {
                DirectoryInfo currentDirectory = DirectoryStack.Pop();

                if(_stopEnumeration)
                    break;

                try
                {
                    foreach(DirectoryInfo DirItem in currentDirectory.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        if(_stopEnumeration)
                            break;

                        try
                        {
                            DirectoryStack.Push(DirItem);
                            OnDirectoryFound(new DirectoryFoundEventArgs(DirItem));
                        }
                        catch(Exception Ex)
                        {
                            Console.WriteLine("Exception: {0}", Ex.Message);
                        }
                    }
                }
                catch(Exception Ex)
                {
                    Console.WriteLine("Exception: {0}", Ex.Message);
                }
            }
        }

        public void EnumerateDirectoriesBFS(string rootDirectory)
        {
            Queue<DirectoryInfo> DirectoryQueue = new Queue<DirectoryInfo>();

            if(rootDirectory == null || rootDirectory.Length == 0)
            {
                return;
            }

            _stopEnumeration = false;
            DirectoryInfo rootDir = new DirectoryInfo(rootDirectory);
            DirectoryQueue.Enqueue(rootDir);

            while(DirectoryQueue.Count != 0)
            {
                DirectoryInfo currentDirectory = DirectoryQueue.Dequeue();

                if(_stopEnumeration)
                    break;

                try
                {
                    foreach(DirectoryInfo DirItem in currentDirectory.EnumerateDirectories("*.*", SearchOption.TopDirectoryOnly))
                    {
                        if(_stopEnumeration)
                            break;

                        try
                        {
                            DirectoryQueue.Enqueue(DirItem);
                            OnDirectoryFound(new DirectoryFoundEventArgs(DirItem));
                        }
                        catch(Exception Ex)
                        {
                            Console.WriteLine("Exception: {0}", Ex.Message);
                        }
                    }
                }
                catch(Exception Ex)
                {
                    Console.WriteLine("Exception: {0}", Ex.Message);
                }
            }
        }

    }
}
