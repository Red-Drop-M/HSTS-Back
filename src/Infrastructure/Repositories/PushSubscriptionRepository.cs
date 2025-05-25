using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PushSubscriptionRepository : IPushSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public PushSubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PushSubscription?> GetByIdAsync(Guid id)
        {
            return await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<PushSubscription?> GetByEndpointAsync(string endpoint)
        {
            return await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        }

        public async Task<List<PushSubscription>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PushSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PushSubscription>> GetAllAsync()
        {
            return await _context.PushSubscriptions.ToListAsync();
        }

        public async Task AddAsync(PushSubscription subscription)
        {
            await _context.PushSubscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PushSubscription subscription)
        {
            _context.PushSubscriptions.Update(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var subscription = await GetByIdAsync(id);
            if (subscription != null)
            {
                _context.PushSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByEndpointAsync(string endpoint)
        {
            var subscription = await GetByEndpointAsync(endpoint);
            if (subscription != null)
            {
                _context.PushSubscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
        }
    }
}