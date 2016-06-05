using System;
using System.ComponentModel;

using DuplicateFileDetectorMVVM.Common;


namespace DuplicateFileDetectorMVVM.Model
{
    public class FileDetail : INotifyBase
    {

        private string _FileName;
        public string FileName
        {
            get { return this._FileName; }
            set
            {
                if(this._FileName != value)
                {
                    this._FileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        private string _FullFilePath;
        public string FullFilePath
        {
            get { return this._FullFilePath; }
            set
            {
                if(this._FullFilePath != value)
                {
                    this._FullFilePath = value;
                    OnPropertyChanged("FullFilePath");
                }
            }
        }

        private UInt64 _Size;
        public UInt64 Size
        {
            get { return this._Size; }
            set
            {
                if(this._Size != value)
                {
                    this._Size = value;
                    OnPropertyChanged("Size");
                }
            }
        }

        private string _Hash;
        public string Hash
        {
            get { return this._Hash; }
            set
            {
                if(this._Hash != value)
                {
                    this._Hash = value;
                    OnPropertyChanged("Hash");
                }
            }
        }


        public FileDetail()
        {
            this.FileName = "Invalid";
            this.FullFilePath = "Invalid";
            this.Size = 0;
            this.Hash = "000-000-000-000";

        }

        public FileDetail(string Hash, string FileName, string FullFilePath, UInt64 Size)
        {
            this.FileName = FileName;
            this.FullFilePath = FullFilePath;
            this.Size = Size;
            this.Hash = Hash;
        }

        public FileDetail(FileDetail fileDetail)
        {
            if(fileDetail != null)
            {
                this.FullFilePath = fileDetail.FullFilePath;
                this.FileName = fileDetail.FileName;
                this.Size = fileDetail.Size;
                this.Hash = fileDetail.Hash;

            }
        }

    }
}
