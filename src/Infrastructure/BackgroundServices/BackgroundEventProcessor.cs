using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Interfaces;

namespace Infrastructure.BackgroundServices
{
    public class BackgroundEventProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<BackgroundEventProcessor> _logger;

        public BackgroundEventProcessor(
            IServiceScopeFactory scopeFactory,
            IBackgroundTaskQueue taskQueue,
            ILogger<BackgroundEventProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _taskQueue = taskQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Event Processor is starting");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    _logger.LogInformation("Background task starting");
                    
                    // Create a new scope for each background work item
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        await workItem(scope, stoppingToken);
                    }
                    
                    _logger.LogInformation("Background task completed successfully");
                }
                catch (OperationCanceledException)
                {
                    // Ignore, as this is expected when stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing background task");
                }
            }
            
            _logger.LogInformation("Background Event Processor is stopping");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Event Processor is stopping");
            
            await base.StopAsync(stoppingToken);
        }
    }
}