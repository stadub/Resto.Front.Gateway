using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SQLiteLogger
{
    public abstract class BatchingLoggerProvider : ILoggerProvider, IDisposable, ISupportExternalScope
    {
        private readonly List<LogMessage> _currentBatch = new List<LogMessage>();
        private readonly TimeSpan _interval;
        private readonly int? _queueSize;
        private readonly int? _batchSize;
        private readonly IDisposable _optionsChangeToken;
        private int _messagesDropped;
        private BlockingCollection<LogMessage> _messageQueue;
        private Task _outputTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _includeScopes;
        private IExternalScopeProvider _scopeProvider;

        internal IExternalScopeProvider ScopeProvider => !this._includeScopes ? (IExternalScopeProvider)null : this._scopeProvider;

        internal BatchingLoggerProvider(IOptionsMonitor<BatchingLoggerOptions> options)
        {
            BatchingLoggerOptions currentValue = options.CurrentValue;
            int? batchSize = currentValue.BatchSize;
            int num = 0;
            if (batchSize.GetValueOrDefault() <= num & batchSize.HasValue)
                throw new ArgumentOutOfRangeException("BatchSize", "BatchSize must be a positive number.");
            if (currentValue.FlushPeriod <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("FlushPeriod", "FlushPeriod must be longer than zero.");
            this._interval = currentValue.FlushPeriod;
            this._batchSize = currentValue.BatchSize;
            this._queueSize = currentValue.BackgroundQueueSize;
            this._optionsChangeToken = options.OnChange<BatchingLoggerOptions>(new Action<BatchingLoggerOptions>(this.UpdateOptions));
            this.UpdateOptions(options.CurrentValue);
        }

        public bool IsEnabled { get; private set; }

        private void UpdateOptions(BatchingLoggerOptions options)
        {
            int num1 = this.IsEnabled ? 1 : 0;
            this.IsEnabled = options.IsEnabled;
            this._includeScopes = options.IncludeScopes;
            int num2 = this.IsEnabled ? 1 : 0;
            if (num1 == num2)
                return;
            if (this.IsEnabled)
                this.Start();
            else
                this.Stop();
        }

        internal abstract Task WriteMessagesAsync(
          IEnumerable<LogMessage> messages,
          CancellationToken token);

        private async Task ProcessLogQueue()
        {
            while (!this._cancellationTokenSource.IsCancellationRequested)
            {
                LogMessage logMessage;
                for (int index = this._batchSize ?? int.MaxValue; index > 0 && this._messageQueue.TryTake(out logMessage); --index)
                    this._currentBatch.Add(logMessage);
                int num = Interlocked.Exchange(ref this._messagesDropped, 0);
                if (num != 0)
                    this._currentBatch.Add(new LogMessage(DateTimeOffset.Now, string.Format("{0} message(s) dropped because of queue size limit. Increase the queue size or decrease logging verbosity to avoid this.{1}", (object)num, (object)Environment.NewLine)));
                if (this._currentBatch.Count > 0)
                {
                    try
                    {
                        await this.WriteMessagesAsync((IEnumerable<LogMessage>)this._currentBatch, this._cancellationTokenSource.Token);
                    }
                    catch
                    {
                    }
                    this._currentBatch.Clear();
                }
                else
                    await this.IntervalAsync(this._interval, this._cancellationTokenSource.Token);
            }
        }

        protected virtual Task IntervalAsync(TimeSpan interval, CancellationToken cancellationToken) => Task.Delay(interval, cancellationToken);

        internal void AddMessage(DateTimeOffset timestamp, string message)
        {
            if (this._messageQueue.IsAddingCompleted)
                return;
            try
            {
                if (this._messageQueue.TryAdd(new LogMessage(timestamp, message), 0, this._cancellationTokenSource.Token))
                    return;
                Interlocked.Increment(ref this._messagesDropped);
            }
            catch
            {
            }
        }

        private void Start()
        {
            this._messageQueue = !this._queueSize.HasValue ? new BlockingCollection<LogMessage>((IProducerConsumerCollection<LogMessage>)new ConcurrentQueue<LogMessage>()) : new BlockingCollection<LogMessage>((IProducerConsumerCollection<LogMessage>)new ConcurrentQueue<LogMessage>(), this._queueSize.Value);
            this._cancellationTokenSource = new CancellationTokenSource();
            this._outputTask = Task.Run(new Func<Task>(this.ProcessLogQueue));
        }

        private void Stop()
        {
            this._cancellationTokenSource.Cancel();
            this._messageQueue.CompleteAdding();
            try
            {
                this._outputTask.Wait(this._interval);
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException)
            {
            }
        }

        public void Dispose()
        {
            this._optionsChangeToken?.Dispose();
            if (!this.IsEnabled)
                return;
            this.Stop();
        }

        public ILogger CreateLogger(string categoryName) => (ILogger)new BatchingLogger(this, categoryName);

        void ISupportExternalScope.SetScopeProvider(
          IExternalScopeProvider scopeProvider)
        {
            this._scopeProvider = scopeProvider;
        }
    }
}
