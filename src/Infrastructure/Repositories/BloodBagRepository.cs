using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class BloodBagRepository : IBloodBagRepository
    {
        private readonly ApplicationDbContext _context;

        public BloodBagRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BloodBag?> GetByIdAsync(Guid id)
        {
            return await _context.BloodBags
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BloodBag?>> GetByBloodGroupAsync(BloodType bloodGroup)
        {
            return await _context.BloodBags
                .Where(b => b.BloodType == bloodGroup)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByBloodBagTypeAsync(BloodBagType bloodBagType)
        {
            return await _context.BloodBags
                .Where(b => b.BloodBagType == bloodBagType)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByExpiryDateAsync(DateOnly expiryDate)
        {
            return await _context.BloodBags
                .Where(b => b.ExpirationDate == expiryDate)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByAcquiredDateAsync(DateOnly acquiredDate)
        {
            return await _context.BloodBags
                .Where(b => b.AcquiredDate == acquiredDate)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByStatusAsync(BloodBagStatus status)
        {
            return await _context.BloodBags
                .Where(b => b.Status == status)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.BloodBags
                .Where(b => b.DonorId == donorId)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<BloodBag?> GetByRequestIdAsync(Guid requestId)
        {
            return await _context.BloodBags
                .FirstOrDefaultAsync(b => b.RequestId == requestId);
        }

        public async Task AddAsync(BloodBag bloodBag)
        {
            await _context.BloodBags.AddAsync(bloodBag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BloodBag bloodBag)
        {
            _context.BloodBags.Update(bloodBag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var bloodBag = await GetByIdAsync(id);
            if (bloodBag != null)
            {
                _context.BloodBags.Remove(bloodBag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<BloodBag>, int)> GetAllAsync(int page, int pageSize, BloodBagFilter filter)
        {
            var query = _context.BloodBags.AsQueryable();

            if (filter.BloodType != null)
            query = query.Where(b => b.BloodType == filter.BloodType);

            if (filter.BloodBagType != null)
            query = query.Where(b => b.BloodBagType == filter.BloodBagType);

            if (filter.ExpirationDate != null)
            query = query.Where(b => filter.ExpirationDate.HasValue && b.ExpirationDate == filter.ExpirationDate.Value);

            if (filter.AcquiredDate != null)
            query = query.Where(b => b.AcquiredDate == filter.AcquiredDate);

            if (filter.Status != null)
            query = query.Where(b => b.Status == filter.Status);

            if (filter.DonorId != null)
            query = query.Where(b => b.DonorId == filter.DonorId);

            if (filter.RequestId != null)
            query = query.Where(b => b.RequestId == filter.RequestId);

            var total = await query.CountAsync();

            var bloodBags = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (bloodBags, total);
        }
    }
}
