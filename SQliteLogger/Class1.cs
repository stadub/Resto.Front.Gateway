using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SQLiteLogger
{
    [ProviderAlias("AzureAppServicesFile")]
    public class FileLoggerProvider : BatchingLoggerProvider
    {
        private readonly string _path;
        private readonly string _fileName;
        private readonly int? _maxFileSize;
        private readonly int? _maxRetainedFiles;

        public FileLoggerProvider(IOptionsMonitor<AzureFileLoggerOptions> options)
            : base((IOptionsMonitor<BatchingLoggerOptions>) options)
        {
            AzureFileLoggerOptions currentValue = options.CurrentValue;
            this._path = currentValue.LogDirectory;
            this._fileName = currentValue.FileName;
            this._maxFileSize = currentValue.FileSizeLimit;
            this._maxRetainedFiles = currentValue.RetainedFileCountLimit;
        }

        internal override async Task WriteMessagesAsync(
            IEnumerable<LogMessage> messages,
            CancellationToken cancellationToken)
        {
            FileLoggerProvider fileLoggerProvider = this;
            Directory.CreateDirectory(fileLoggerProvider._path);
            foreach (IGrouping<(int, int, int), LogMessage> grouping in messages.GroupBy<LogMessage, (int, int, int)>(
                new Func<LogMessage, (int, int, int)>(fileLoggerProvider.GetGrouping)))
            {
                string fullName = fileLoggerProvider.GetFullName(grouping.Key);
                FileInfo fileInfo = new FileInfo(fullName);
                int? maxFileSize = fileLoggerProvider._maxFileSize;
                int num = 0;
                if (maxFileSize.GetValueOrDefault() > num & maxFileSize.HasValue && fileInfo.Exists)
                {
                    long length = fileInfo.Length;
                    maxFileSize = fileLoggerProvider._maxFileSize;
                    long? nullable = maxFileSize.HasValue
                        ? new long?((long) maxFileSize.GetValueOrDefault())
                        : new long?();
                    long valueOrDefault = nullable.GetValueOrDefault();
                    if (length > valueOrDefault & nullable.HasValue)
                        return;
                }

                using (StreamWriter streamWriter = File.AppendText(fullName))
                {
                    foreach (LogMessage logMessage in (IEnumerable<LogMessage>) grouping)
                        await streamWriter.WriteAsync(logMessage.Message);
                }
            }

            fileLoggerProvider.RollFiles();
        }

        private string GetFullName((int Year, int Month, int Day) group) => Path.Combine(this._path,
            string.Format("{0}{1:0000}{2:00}{3:00}.txt", (object) this._fileName, (object) group.Year,
                (object) group.Month, (object) group.Day));

        private (int Year, int Month, int Day) GetGrouping(LogMessage message)
        {
            DateTimeOffset timestamp = message.Timestamp;
            int year = timestamp.Year;
            timestamp = message.Timestamp;
            int month = timestamp.Month;
            timestamp = message.Timestamp;
            int day = timestamp.Day;
            return (year, month, day);
        }

        private void RollFiles()
        {
            int? maxRetainedFiles = this._maxRetainedFiles;
            int num = 0;
            if (!(maxRetainedFiles.GetValueOrDefault() > num & maxRetainedFiles.HasValue))
                return;
            foreach (FileSystemInfo fileSystemInfo in
                ((IEnumerable<FileInfo>) new DirectoryInfo(this._path).GetFiles(this._fileName + "*"))
                .OrderByDescending<FileInfo, string>((Func<FileInfo, string>) (f => f.Name))
                .Skip<FileInfo>(this._maxRetainedFiles.Value))
                fileSystemInfo.Delete();
        }
    }
}
