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
    
        public async Task<IEnumerable<DonorPledge>> GetByDonorNameAsync(Guid donorId)
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

        public async Task<(List<DonorPledge> pledges, int total)> GetAllAsync(
    int page,
    int pageSize,
    string? status = null,
    Guid? donorId = null,
    Guid? requestId = null,
    string? bloodType = null)
{
    var query = _context.Pledges
        .Include(p => p.Donor)
        .Include(p => p.Request)
        .AsQueryable();

    // Apply filters
    if (!string.IsNullOrEmpty(status))
    {
        query = query.Where(p => p.Status.Value == status);
    }

    if (donorId.HasValue && donorId.Value != Guid.Empty)
    {
        query = query.Where(p => p.DonorId == donorId.Value);
    }

    if (requestId.HasValue && requestId.Value != Guid.Empty)
    {
        query = query.Where(p => p.RequestId == requestId.Value);
    }

    if (!string.IsNullOrEmpty(bloodType))
    {
        query = query.Where(p => p.Request.BloodType.Value == bloodType);
    }

    // Get total count
    var total = await query.CountAsync();

    // Apply pagination
    var pledges = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (pledges, total);
}

public async Task<DonorPledge?> GetByDonorAndRequestIdAsync(Guid donorId, Guid requestId)
{
    return await _context.Pledges
        .FirstOrDefaultAsync(p => p.DonorId == donorId && p.RequestId == requestId);
}
    }
}