using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IPushSubscriptionRepository
    {
        Task<PushSubscription?> GetByIdAsync(Guid id);
        Task<PushSubscription?> GetByEndpointAsync(string endpoint);
        Task<List<PushSubscription>> GetByUserIdAsync(Guid userId);
        Task<List<PushSubscription>> GetAllAsync();
        Task AddAsync(PushSubscription subscription);
        Task UpdateAsync(PushSubscription subscription);
        Task DeleteAsync(Guid id);
        Task DeleteByEndpointAsync(string endpoint);
    }
}