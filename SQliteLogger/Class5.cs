using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteLogger
{
    public class AzureFileLoggerOptions : BatchingLoggerOptions
    {
        private int? _fileSizeLimit = new int?(10485760);
        private int? _retainedFileCountLimit = new int?(2);
        private string _fileName = "diagnostics-";

        public int? FileSizeLimit
        {
            get => this._fileSizeLimit;
            set
            {
                int? nullable = value;
                int num = 0;
                if (nullable.GetValueOrDefault() <= num & nullable.HasValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "FileSizeLimit must be positive.");
                this._fileSizeLimit = value;
            }
        }

        public int? RetainedFileCountLimit
        {
            get => this._retainedFileCountLimit;
            set
            {
                int? nullable = value;
                int num = 0;
                if (nullable.GetValueOrDefault() <= num & nullable.HasValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "RetainedFileCountLimit must be positive.");
                this._retainedFileCountLimit = value;
            }
        }

        public string FileName
        {
            get => this._fileName;
            set => this._fileName = !string.IsNullOrEmpty(value) ? value : throw new ArgumentException(nameof(value));
        }

        internal string LogDirectory { get; set; }
    }
}
