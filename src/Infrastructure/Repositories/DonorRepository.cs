using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class DonorRepository : IDonorRepository
    {
        private readonly ApplicationDbContext _context;

        public DonorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Donor?> GetByIdAsync(Guid id)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Donor?> GetByEmailAsync(string email)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.Email == email);
        }

        public async Task<Donor?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.PhoneNumber == phoneNumber);
        }

        public async Task<Donor?> GetByAddressAsync(string address)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.Address == address);
        }

        public async Task<Donor?> GetByNINAsync(string nin)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.NIN == nin);
        }

        public async Task<Donor?> GetByNameAsync(string name)
        {
            return await _context.Donors
                .FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<List<Donor>> GetByBloodGroupAsync(BloodType bloodGroup)
        {
            return await _context.Donors
                .Where(d => d.BloodType == bloodGroup)
                .ToListAsync();
        }

        public async Task<List<Donor>> GetByLastDonnationDateAsync(DateOnly date)
        {
            return await _context.Donors
                .Where(d => d.LastDonationDate == date)
                .ToListAsync();
        }

        public async Task AddAsync(Donor donor)
        {
            await _context.Donors.AddAsync(donor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Donor donor)
        {
            _context.Donors.Update(donor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var donor = await GetByIdAsync(id);
            if (donor != null)
            {
                _context.Donors.Remove(donor);
                await _context.SaveChangesAsync();
            }
        }

         public async Task<List<Donor>> GetAllAsync()
        {
            return await _context.Donors
                .ToListAsync();
        }

        public async Task<(List<Donor>, int)> GetAllAsync(int page, int pageSize, DonorFilter filter)
        {
            var query = _context.Donors.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(d => d.Name.Contains(filter.Name));

            if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(d => d.Email.Contains(filter.Email));

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
            query = query.Where(d => d.PhoneNumber.Contains(filter.PhoneNumber));

            if (!string.IsNullOrEmpty(filter.Address))
            query = query.Where(d => d.Address.Contains(filter.Address));

            if (!string.IsNullOrEmpty(filter.NIN))
            query = query.Where(d => d.NIN.Contains(filter.NIN));

            if (filter.BloodType != null)
            query = query.Where(d => d.BloodType == filter.BloodType);

            if (filter.LastDonationDate != null)
            query = query.Where(d => d.LastDonationDate == filter.LastDonationDate);

            var total = await query.CountAsync();

            var donors = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (donors, total);
        }

       
    }
}
