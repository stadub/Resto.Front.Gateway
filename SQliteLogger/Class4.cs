using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteLogger
{
    public class BatchingLoggerOptions
    {
        private int? _batchSize;
        private int? _backgroundQueueSize = new int?(1000);
        private TimeSpan _flushPeriod = TimeSpan.FromSeconds(1.0);

        public TimeSpan FlushPeriod
        {
            get => this._flushPeriod;
            set => this._flushPeriod = !(value <= TimeSpan.Zero) ? value : throw new ArgumentOutOfRangeException(nameof(value), "FlushPeriod must be positive.");
        }

        public int? BackgroundQueueSize
        {
            get => this._backgroundQueueSize;
            set
            {
                int? nullable = value;
                int num = 0;
                if (nullable.GetValueOrDefault() < num & nullable.HasValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "BackgroundQueueSize must be non-negative.");
                this._backgroundQueueSize = value;
            }
        }

        public int? BatchSize
        {
            get => this._batchSize;
            set
            {
                int? nullable = value;
                int num = 0;
                if (nullable.GetValueOrDefault() <= num & nullable.HasValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "BatchSize must be positive.");
                this._batchSize = value;
            }
        }

        public bool IsEnabled { get; set; }

        public bool IncludeScopes { get; set; }
    }
}
