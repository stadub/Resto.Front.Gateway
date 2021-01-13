using System;
using Microsoft.Extensions.Logging;

namespace SQLiteLogger
{
    public static class SQLiteLoggerExtensions
    {
        public static ILoggingBuilder AddSQLiteLogger(
            this ILoggingBuilder builder) =>
            builder.AddSQLiteLogger(
                new SQLiteLoggerSettings());

        public static ILoggingBuilder AddSQLiteLogger(
            this ILoggingBuilder builder,
            Action<SQLiteLoggerSettings> configure)
        {
            var config = new SQLiteLoggerSettings();
            configure(config);

            return builder.AddSQLiteLogger(config);
        }

        public static ILoggingBuilder AddSQLiteLogger(
            this ILoggingBuilder builder,
            SQLiteLoggerSettings config)
        {
            builder.AddProvider(new SQLiteLoggerProvider(config));
            return builder;
        }
    }
}