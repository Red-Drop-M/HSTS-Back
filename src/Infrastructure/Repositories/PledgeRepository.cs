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

        public async Task<DonorPledge?> GetByIdAsync(Guid donorId, Guid requestId)
        {   
                return await _context.Pledges
                .FirstOrDefaultAsync(p => p.DonorId == donorId && p.RequestId == requestId);
        }   

        public async Task<IEnumerable<DonorPledge>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Pledges
                .Where(p => p.DonorId == donorId)
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

        public async Task DeleteAsync(Guid donorId, Guid requestId)
        {
            var pledge = await GetByIdAsync(donorId, requestId);
            if (pledge != null)
            {
                _context.Pledges.Remove(pledge);
                await _context.SaveChangesAsync();
            }
        }

    }
}