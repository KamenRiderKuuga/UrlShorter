using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using FASTER.core;

namespace UrlShorter
{
    public class TimedCheckpointService : IHostedService, IDisposable
    {
        private readonly FasterKV<long, string> _store;
        private readonly ILogger<TimedCheckpointService> _logger;
        private Timer _timer = null;

        public TimedCheckpointService(FasterKV<long, string> store, ILogger<TimedCheckpointService> logger)
        {
            _store = store;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            await _store.TakeFullCheckpointAsync(CheckpointType.FoldOver);
            _logger.LogInformation(
                $"Timed Checkpoint Service is working, time: {DateTimeOffset.UtcNow}");
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Checkpoint Service is stopping.");
            await _store.TakeFullCheckpointAsync(CheckpointType.FoldOver);
            _timer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
