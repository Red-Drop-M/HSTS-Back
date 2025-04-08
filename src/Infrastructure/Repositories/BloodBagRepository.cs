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
                .Where(b => b.BloodGroup == bloodGroup)
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
                .Where(b => b.ExpiryDate == expiryDate)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByDonationDateAsync(DateOnly donationDate)
        {
            return await _context.BloodBags
                .Where(b => b.DonationDate == donationDate)
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
                .FirstOrDefaultAsync(b => b.DonorId == donorId);
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
    }
}
