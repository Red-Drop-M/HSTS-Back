using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Interfaces
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync(Func<IServiceScope, CancellationToken, ValueTask> workItem);
        
        ValueTask<Func<IServiceScope, CancellationToken, ValueTask>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}