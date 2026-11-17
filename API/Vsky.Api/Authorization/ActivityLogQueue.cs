using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vsky.Models;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Saves routine activity rows in batches, so a request doesn't wait for a database insert.
    /// Exceptions and security events don't use this queue: they're written immediately.
    /// </summary>
    public class ActivityLogQueue : BackgroundService
    {
        private const int Capacity = 10000;
        private const int BatchSize = 200;
        private static readonly TimeSpan BatchInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan DrainLimit = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan DropReportInterval = TimeSpan.FromMinutes(1);

        private readonly SecurityLog _securityLog;
        private readonly ILogger<ActivityLogQueue> _logger;
        private readonly Channel<Log> _channel = Channel.CreateBounded<Log>(new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = false
        });

        private volatile bool _stopping;
        private long _dropped;
        private DateTime _lastDropReportUtc = DateTime.MinValue;

        public ActivityLogQueue(SecurityLog securityLog, ILogger<ActivityLogQueue> logger)
        {
            _securityLog = securityLog;
            _logger = logger;
        }

        /// <summary>Never blocks and never throws. When the queue is full, routine activity is dropped and counted.</summary>
        public void Enqueue(Log row)
        {
            if (_channel.Writer.TryWrite(row))
                return;

            if (_stopping)
                _securityLog.WriteToFallback(new[] { row });
            else
                Interlocked.Increment(ref _dropped);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _channel.Reader;
            var batch = new List<Log>(BatchSize);

            // Ends when StopAsync closes the channel and it's empty, not when the token fires.
            while (await reader.WaitToReadAsync(CancellationToken.None))
            {
                try
                {
                    if (!_stopping && reader.Count < BatchSize)
                        await Task.WhenAny(Task.Delay(BatchInterval, stoppingToken));

                    batch.Clear();
                    while (batch.Count < BatchSize && reader.TryRead(out var row))
                        batch.Add(row);

                    _securityLog.WriteBatch(batch);
                    ReportDroppedRows();
                }
                catch (Exception ex)
                {
                    // An exception leaving this method would stop the whole application.
                    _logger.LogWarning("Activity log batch failed: {Type}", ex.GetType().Name);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopping = true;
            _channel.Writer.TryComplete();

            try
            {
                using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                limit.CancelAfter(DrainLimit);
                await base.StopAsync(limit.Token);
            }
            catch (OperationCanceledException)
            {
                // The time limit passed. Whatever is left goes to the fallback below.
            }

            var remaining = new List<Log>();
            while (_channel.Reader.TryRead(out var row))
                remaining.Add(row);

            if (remaining.Count > 0)
                _securityLog.WriteToFallback(remaining);

            ReportDroppedRows();
        }

        private void ReportDroppedRows()
        {
            if (Interlocked.Read(ref _dropped) == 0 || DateTime.UtcNow - _lastDropReportUtc < DropReportInterval)
                return;

            _lastDropReportUtc = DateTime.UtcNow;
            _logger.LogWarning("The activity log queue was full. {Count} rows were dropped.", Interlocked.Exchange(ref _dropped, 0));
        }
    }
}
