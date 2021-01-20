using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteLogger
{
    internal readonly struct LogMessage
    {
        public LogMessage(DateTimeOffset timestamp, string message)
        {
            this.Timestamp = timestamp;
            this.Message = message;
        }

        public DateTimeOffset Timestamp { get; }

        public string Message { get; }
    }
}
