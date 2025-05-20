// src/Infrastructure/BackgroundServices/DailySchedulerService.cs

using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HSTS_Back.Infrastructure.BackgroundServices
{
    public class DailySchedulerService : BackgroundService
    {
        private readonly ILogger<DailySchedulerService> _logger;
        private readonly IServiceScopeFactory _scopeFactory; // Needed to create scopes for MediatR dispatching

        // Define your scheduled tasks and their times
        // You could even load these from configuration or a database if they change frequently
        private static readonly (TimeSpan Time, Func<IRequest> CommandFactory)[] ScheduledTasks = new (TimeSpan, Func<IRequest>)[]
        {
            (new TimeSpan(2, 0, 0), () => new Application.Features.BloodBagManagement.Commands.CleanupExpiredBloodBagsCommand()), // Example for 2 AM
            (new TimeSpan(4, 30, 0), () => new Application.Features.BloodRequests.Commands.CleanupExpiredRequestsCommand()), // Example for 4:30 AM
            (new TimeSpan(23, 0, 0), () => new Application.Features.PledgeManagement.Commands.CleanupExpiredPledgesCommand()) // Example for 11 PM
        };

        public DailySchedulerService(ILogger<DailySchedulerService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Scheduler Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Calculate the delay until the next minute mark for precise checking
                var now = DateTime.Now;
                var nextCheckTime = now.AddMinutes(1).Subtract(TimeSpan.FromSeconds(now.Second)).Subtract(TimeSpan.FromMilliseconds(now.Millisecond));
                var initialDelay = nextCheckTime - now;

                if (initialDelay < TimeSpan.Zero)
                {
                    // This can happen if the service starts precisely on a minute mark,
                    // or if the calculation pushes it slightly into the past.
                    initialDelay = TimeSpan.FromSeconds(1); // Wait for a short moment
                }
                
                _logger.LogDebug("Next schedule check in: {InitialDelay}", initialDelay);
                
                try
                {
                    await Task.Delay(initialDelay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Daily Scheduler Service is stopping gracefully.");
                    break;
                }

                // Now, check if any tasks are due to run in the current minute
                await CheckAndDispatchCommands(stoppingToken);

                // After the initial check, subsequent checks can be every minute
                // This ensures we hit the precise minute mark for scheduled tasks
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); 
            }
        }

        private async Task CheckAndDispatchCommands(CancellationToken stoppingToken)
        {
            var currentTime = DateTime.Now;
            var currentMinute = currentTime.TimeOfDay.Subtract(TimeSpan.FromSeconds(currentTime.Second)).Subtract(TimeSpan.FromMilliseconds(currentTime.Millisecond));

            foreach (var task in ScheduledTasks)
            {
                // Compare only up to the minute for scheduling accuracy
                var scheduledMinute = task.Time.Subtract(TimeSpan.FromSeconds(task.Time.Seconds)).Subtract(TimeSpan.FromMilliseconds(task.Time.Milliseconds));

                // Check if the current minute matches the scheduled minute
                if (currentMinute == scheduledMinute)
                {
                    _logger.LogInformation("Daily Scheduler: Attempting to dispatch command for {Time}", task.Time);
                    await DispatchCommandInScope(task.CommandFactory(), stoppingToken);
                }
            }
        }

        private async Task DispatchCommandInScope(IRequest command, CancellationToken stoppingToken)
        {
            // IMPORTANT: Create a new scope for each command dispatch.
            // This is crucial because MediatR handlers (and their dependencies like DbContext)
            // are often scoped. The BackgroundService itself is a singleton.
            using (var scope = _scopeFactory.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                try
                {
                    // Assuming your commands don't return a value for these background tasks.
                    // If they do, you'll need to adjust IRequest to IRequest<TResponse>
                    // and use mediator.Send(command) accordingly.
                    await mediator.Send(command, stoppingToken);
                    _logger.LogInformation("Successfully dispatched and executed command: {CommandType}", command.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching or executing command {CommandType}", command.GetType().Name);
                    // Consider implementing retry logic here if necessary
                }
            }
        }
    }
}