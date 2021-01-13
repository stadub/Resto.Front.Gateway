﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SQLiteLogger
{
    [ProviderAlias("SQLiteProvider")]
    public sealed class SQLiteLoggerProvider : ILoggerProvider
    {
        private readonly SQLiteLoggerSettings _config;
        private readonly ConcurrentDictionary<string, SQLiteLogger> _loggers =
            new ConcurrentDictionary<string, SQLiteLogger>();


        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SQLiteLoggerProvider(ISqlBatchLogTask logger, SQLiteLoggerSettings settings)
        {
            this._settings = settings;
            _logger = logger;
        }

        /// <summary>
        /// Creates SQL Logger Provider 
        /// </summary>
        /// <param name="settings">Logger Settings</param>
        /// <param name="filter">TODO..</param>
        public SQLiteLoggerProvider(ISqlBatchLogTask logger, IOptions<SQLiteLoggerSettings> settings) : this(logger, settings.Value)
        {
        }

        public SQLiteLoggerProvider(SQLiteLoggerSettings config) =>
            _config = config;



        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new SQLiteLogger(name, _config));

        public void Dispose() => _loggers.Clear();
    }
}