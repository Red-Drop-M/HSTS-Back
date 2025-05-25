using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WilayaRepository : IWilayaRepository
    {
        private readonly ApplicationDbContext _context;

        public WilayaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wilaya?> GetByIdAsync(Guid id)
        {
            return await _context.Wilayas
                .Include(w => w.Communes)
                .Include(w => w.BloodTransferCenters)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Wilaya?> GetByNameAsync(string name)
        {
            return await _context.Wilayas
                .Include(w => w.Communes)
                .Include(w => w.BloodTransferCenters)
                .FirstOrDefaultAsync(w => w.Name == name);
        }

        public async Task<List<Wilaya>> GetAllAsync()
        {
            return await _context.Wilayas
                .Include(w => w.Communes)
                .Include(w => w.BloodTransferCenters)
                .ToListAsync();
        }

        public async Task<(List<Wilaya> Wilayas, int Total)> GetAllAsync(int page, int pageSize, string? name = null)
        {
            var query = _context.Wilayas
                .Include(w => w.Communes)
                .Include(w => w.BloodTransferCenters)
                .AsQueryable();

            // Apply name filter if provided
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(w => w.Name.Contains(name));
            }

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination
            var wilayas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (wilayas, total);
        }

        public async Task<Wilaya> AddAsync(Wilaya wilaya)
        {
            await _context.Wilayas.AddAsync(wilaya);
            await _context.SaveChangesAsync();
            return wilaya;
        }

        public async Task UpdateAsync(Wilaya wilaya)
        {
            _context.Wilayas.Update(wilaya);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var wilaya = await GetByIdAsync(id);
            if (wilaya != null)
            {
                _context.Wilayas.Remove(wilaya);
                await _context.SaveChangesAsync();
            }
        }
    }
}