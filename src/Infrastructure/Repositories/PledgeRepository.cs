using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Repositories;
namespace Infrastructure.Repositories
{
    public class PledgeRepository : IPledgeRepository
{
        private readonly ApplicationDbContext _context;

        public PledgeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DonorPledge?> GetByIdAsync(string donorName, Guid requestId)
        {   
                return await _context.Pledges
                .FirstOrDefaultAsync(p => p.DonorName == donorName && p.RequestId == requestId);
        }   
    
        public async Task<IEnumerable<DonorPledge>> GetByDonorNameAsync(string donorName)
        {
            return await _context.Pledges
                .Where(p => p.DonorName == donorName)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonorPledge>> GetByRequestIdAsync(Guid requestId)
        {
            return await _context.Pledges
                .Where(p => p.RequestId == requestId)
                .ToListAsync();
        }

        public async Task AddAsync(DonorPledge pledge)
        {
            await _context.Pledges.AddAsync(pledge);
            await _context.SaveChangesAsync();
        }   

        public async Task UpdateAsync(DonorPledge pledge)
        {
            _context.Pledges.Update(pledge);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string donorName, Guid requestId)
        {
            var pledge = await GetByIdAsync(donorName, requestId);
            if (pledge != null)
            {
                _context.Pledges.Remove(pledge);
                await _context.SaveChangesAsync();
            }
        }

    }
}