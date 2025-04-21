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

        public async Task<List<BloodBag?>> GetByDonationDateAsync(DateOnly donationDate)
        {
            return await _context.BloodBags
                .Where(b => b.AcquiredDate == donationDate)
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
            query = query.Where(b => filter.ExpirationDate.HasValue && b.ExpirationDate == DateOnly.FromDateTime(filter.ExpirationDate.Value));

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


        /*
        public async Task<(List<Request>, int)> GetAllAsync(int page, int pageSize, RequestFilter filter)
        {
            var query = _context.Requests.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Priority))

            query = query.Where(r => r.Priority.Value == filter.Priority);

            if (!string.IsNullOrEmpty(filter.BloodBagType))
            query = query.Where(r => r.BloodBagType.Value == filter.BloodBagType);
            if(!string.IsNullOrEmpty(filter.BloodGroup))
            query = query.Where(r => r.BloodGroup.Value == filter.BloodGroup);
            if (filter.RequestDate != null)
            query = query.Where(r => r.RequestDate == DateOnly.Parse(filter.RequestDate));
            if (filter.DueDate != null)
            query = query.Where(r => r.DueDate == DateOnly.Parse(filter.DueDate));
            if (filter.DonorId != null)
            query = query.Where(r => r.DonorId == Guid.Parse(filter.DonorId));
            if (filter.ServiceId != null)
            query = query.Where(r => r.ServiceId == Guid.Parse(filter.ServiceId));
            if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(r => r.Status.Value == filter.Status);

            var total = await query.CountAsync();

            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (requests, total);
        }
        */

    }
}
