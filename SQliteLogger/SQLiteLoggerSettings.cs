﻿using System;
using Microsoft.Extensions.Logging;

namespace SQLiteLogger
{
    public class SQLiteLoggerSettings
    {
        string Table { get; }
        string Database { get; }

        bool UseBatching { get; set; }

        int BatchSize { get; set; }



        public int EventId { get; set; }
        public LogLevel LogLevel { get; set; } =
            LogLevel.Information | LogLevel.Warning | LogLevel.Error | LogLevel.Critical;

        public ConsoleColor Color { get; set; } = ConsoleColor.Green;
    }
}