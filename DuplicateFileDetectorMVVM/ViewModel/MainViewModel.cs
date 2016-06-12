using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Threading;
using System.Windows;


using DuplicateFileDetectorMVVM.Model;
using DuplicateFileDetectorMVVM.Commands;
using DuplicateFileDetectorMVVM.IO;
using DuplicateFileDetectorMVVM.Hashing;


namespace DuplicateFileDetectorMVVM.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region Properties

        private FileDetail _FileEnumCurrentFile;
        public FileDetail FileEnumCurrentFile
        {
            get { return _FileEnumCurrentFile; }
            set
            {
                _FileEnumCurrentFile = value; base.OnPropertyChanged("FileEnumCurrentFile");
            }
        }

        private string _FileEnumTotalTimeForScan;
        public string FileEnumTotalTimeForScan
        {
            get { return _FileEnumTotalTimeForScan; }
            set { _FileEnumTotalTimeForScan = value; base.OnPropertyChanged("FileEnumTotalTimeForScan"); }
        }

        private UInt64 _FileEnumTotalFilesScanned;
        public UInt64 FileEnumTotalFilesScanned
        {
            get { return _FileEnumTotalFilesScanned; }
            set { _FileEnumTotalFilesScanned = value; base.OnPropertyChanged("FileEnumTotalFilesScanned"); }
        }

        private FileDetail _HashCurrentFile;
        public FileDetail HashCurrentFile
        {
            get { return _HashCurrentFile; }
            set { _HashCurrentFile = value; base.OnPropertyChanged("HashCurrentFile"); }
        }

        private ObservableCollection<string> _ErrorList;

        public ObservableCollection<string> ErrorList
        {
            get { return _ErrorList; }
            set { _ErrorList = value; base.OnPropertyChanged("ErrorList"); }
        }

        private ObservableCollection<FileDetail> _DuplicateFileList;
        public ObservableCollection<FileDetail> DuplicateFileList
        {
            get { return _DuplicateFileList; }
            set { _DuplicateFileList = value; base.OnPropertyChanged("DuplicateFileList"); }
        }

        private string _FolderPath;
        public string FolderPath
        {
            get { return _FolderPath; }
            set { _FolderPath = value; base.OnPropertyChanged("FolderPath"); }
        }

        private bool _CanScanFiles;
        public bool CanScanFiles
        {
            get { return _CanScanFiles; }
            set { _CanScanFiles = value; base.OnPropertyChanged("CanScanFiles"); }
        }

        private string _ScanButtonCaption;
        public string ScanButtonCaption
        {
            get { return _ScanButtonCaption; }
            set { _ScanButtonCaption = value; base.OnPropertyChanged("ScanButtonCaption"); }
        }

        private bool _IsScanInProgress;
        public bool IsScanInProgress
        {
            get { return _IsScanInProgress; }
            set { _IsScanInProgress = value; base.OnPropertyChanged("IsScanInProgress"); }
        }

        private int _NumberOfBuckets;

        public int NumberOfBuckets
        {
            get { return _NumberOfBuckets; }
            set { _NumberOfBuckets = value; base.OnPropertyChanged("NumberOfBuckets"); }
        }

        private ICommand _CmdBeginScan;
        public ICommand CmdBeginScan
        {
            get
            {
                if (_CmdBeginScan == null)
                {
                    _CmdBeginScan = new RelayCommand(
                        param => this.CmdBeginScanHandler()
                        );
                }
                System.Diagnostics.Debug.WriteLine("_CmdBeginScan..");
                return _CmdBeginScan;
            }
        }

        private ICommand _CmdSetFolderPath;
        public ICommand CmdSetFolderPath
        {
            get
            {
                if (_CmdSetFolderPath == null)
                {
                    _CmdSetFolderPath = new RelayCommand(
                        param => this.CmdSetFolderPathHandler(param)
                        );
                }
                System.Diagnostics.Debug.WriteLine("_CmdSetFolderPath..");
                return _CmdSetFolderPath;
            }
        }

        private ICommand _CmdDeleteSelectedList;
        public ICommand CmdDeleteSelectedList
        {
            get
            {
                if (_CmdDeleteSelectedList == null)
                {
                    _CmdDeleteSelectedList = new RelayCommand(
                        param => this.CmdDeleteSelectedListHandler(param)
                        );
                }
                System.Diagnostics.Debug.WriteLine("_CmdDeleteSelectedList..");
                return _CmdDeleteSelectedList;
            }
        }


        #endregion

        #region privates

        DirFileEnumeration _dirFileEnumeration;

        Dictionary<UInt64, FileDetail> _FileSizeInfo = new Dictionary<UInt64, FileDetail>();

        Dictionary<string, List<FileDetail>> _HashInfoFiles = new Dictionary<string, List<FileDetail>>();

        DateTime _start;
        DateTime _end;

        #endregion

        void CmdSetFolderPathHandler(object param)
        {
            System.Diagnostics.Debug.WriteLine("CmdBeginScanHandler..");

            if (Directory.Exists(param as string))
            {
                FolderPath = param as string;
                CanScanFiles = true;
            }
            else
            {
                FolderPath = "Invalid folder path";
                CanScanFiles = false;
                this.IsScanInProgress = false;
            }
        }


        async void CmdBeginScanHandler()
        {
            System.Diagnostics.Debug.WriteLine("CmdBeginScanHandler..");
            //ThreadPool.QueueUserWorkItem(ThreadPoolWorkerFileEnumeration, this);
            await Task.Run(() => ThreadPoolWorkerFileEnumeration(this));
        }


        void CmdDeleteSelectedListHandler(object param)
        {
            System.Diagnostics.Debug.WriteLine("CmdDeleteSelectedListHandler..");
            ThreadPool.QueueUserWorkItem(ThreadPoolWorkerDeleteSelectedItems, param);

        }


        public MainViewModel()
        {
            DisplayName = "MainViewModel";
            DuplicateFileList = new ObservableCollection<FileDetail>();
            ErrorList = new ObservableCollection<string>();

            _dirFileEnumeration = new DirFileEnumeration(this);
            _dirFileEnumeration.FileFound += _dirFileEnumeration_FileFound;

            this.CanScanFiles = false;
            this.ScanButtonCaption = "Begin Scan";
            this.IsScanInProgress = false;

        }


        void _dirFileEnumeration_FileFound(object sender, FileFoundEventArgs e)
        {
            /* _HashInfoFiles is dictionary containing files of different hashes.
             * Key is hash and value is list of file details with same hash.
             * _FileSizeInfo contains key-value of file size and one file detail.
             * _FileSizeInfo basically helps check if there are any duplicate files
             * based on the file size. If yes, then file detail will be added in 
             * _HashInfoFiles dictionary.
             */

            FileEnumTotalFilesScanned++;

            FileEnumCurrentFile = new FileDetail("", e.fileInfo.Name, e.fileInfo.FullName, (UInt64)e.fileInfo.Length);

            FileDetail fileDetail = null;

            if (_FileSizeInfo.TryGetValue(FileEnumCurrentFile.Size, out fileDetail))
            {
                /*
                 * We found a duplicate entry if we reach this code section. 
                 * Get the first file from _FileSizeInfo dictionary. Check if hash value is calculated.
                 * If not, this is the first entry so calculate the hash value and then add it to 
                 * _HashInfoFiles dictionary based on the hash. This will be the first hash entry in the
                 * _HashInfoFiles dictionary
                 */
                List<FileDetail> list = null;
                if (string.IsNullOrEmpty(fileDetail.Hash))
                {
                    fileDetail.Hash = Hashing.MD5Hash.GetMD5HashFromFile(fileDetail.FullFilePath);
                    HashCurrentFile = fileDetail;
                    list = new List<FileDetail>();
                    list.Add(fileDetail);
                    _HashInfoFiles.Add(fileDetail.Hash, list);
                }

                list = null;

                /* 
                 * This is second entry or subsequent file, so calculate the hash for this file.
                 * Then check if that hash exists in the _HashInfoFiles dictionary. If yes, then simply
                 * add it to the list of files denoted by value in dictionary.
                 * If this is unique hash then simply create a key,value entry and add it to _HashInfoFiles. 
                 */
                FileEnumCurrentFile.Hash = Hashing.MD5Hash.GetMD5HashFromFile(FileEnumCurrentFile.FullFilePath);
                HashCurrentFile = FileEnumCurrentFile;

                if (_HashInfoFiles.TryGetValue(FileEnumCurrentFile.Hash, out list))
                {
                    list.Add(FileEnumCurrentFile);
                }
                else
                {
                    list = new List<FileDetail>();
                    list.Add(FileEnumCurrentFile);
                    _HashInfoFiles.Add(FileEnumCurrentFile.Hash, list);
                }

            }
            else
            {
                /*
                * If there is no key with the file size, then simply add the file info to the
                * _FileSizeInfo dictionary without doing any hash calculation. Hash will be calculated
                * only if another match is found.
                */
                fileDetail = new FileDetail(FileEnumCurrentFile);
                _FileSizeInfo.Add(FileEnumCurrentFile.Size, fileDetail);
                NumberOfBuckets = _FileSizeInfo.Count;
            }
        }


        void ThreadPoolWorkerFileEnumeration(object state)
        {
            System.Diagnostics.Debug.WriteLine("ThreadPoolWorkerFileEnumeration:: Start");

            FileEnumTotalFilesScanned = 0;
            FileEnumTotalTimeForScan = "";
            NumberOfBuckets = 0;

            IsScanInProgress = true;
            _FileSizeInfo.Clear();
            _HashInfoFiles.Clear();

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                // Observable collection needs to be accessed in UI thread.
                DuplicateFileList.Clear();
                ErrorList.Clear();
            }));

            _start = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("START:: " + _start.ToString());


            MainViewModel mvm = state as MainViewModel;
            _dirFileEnumeration.EnumerateFiles(mvm.FolderPath);

            /* remove single entries of hashes
             * loop in reverse (bottom up) else when removing index from start, ordering gets corrupted. 
             */
            for (int i = _HashInfoFiles.Count - 1; i >= 0; i--)
            {
                KeyValuePair<string, List<FileDetail>> keyvalue = _HashInfoFiles.ElementAt(i);

                if (keyvalue.Value.Count < 2)
                {
                    _HashInfoFiles.Remove(keyvalue.Key);
                }
                else
                {
                    foreach (var item in keyvalue.Value)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            // access in UI thread.
                            DuplicateFileList.Add(item);
                        }));

                    }
                }
            }

            _FileSizeInfo.Clear();
            _HashInfoFiles.Clear();

            _end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("END:: " + _end.ToString());
            // 3 decimal only
            FileEnumTotalTimeForScan = (_end - _start).ToString();

            System.Diagnostics.Debug.WriteLine("TOTAL ELAPSED TIME:: " + (_end - _start).ToString());

            IsScanInProgress = false;
            System.Diagnostics.Debug.WriteLine("ThreadPoolWorkerFileEnumeration:: End");
        }


        void ThreadPoolWorkerDeleteSelectedItems(object state)
        {
            System.Collections.IList list = (System.Collections.IList)state;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                FileDetail fd = (FileDetail)list[i];

                try
                {
                    File.Delete(fd.FullFilePath);

                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        // access in UI thread.
                        DuplicateFileList.Remove(fd);
                    }));
                }
                catch (Exception Ex)
                {
                    /*
                     Can use some status bar binding to communicate errors in deleting in files
                     Messagebox might not be preferred or maybe list of errors. 
                     */
                    ErrorList.Add(Ex.Message);
                    System.Diagnostics.Debug.WriteLine(Ex.Message);
                }

            }

            System.Diagnostics.Debug.WriteLine("ThreadPoolWorkerDeleteSelectedItems..");
        }
    }
}
