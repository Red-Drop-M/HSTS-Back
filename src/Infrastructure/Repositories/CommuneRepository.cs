using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CommuneRepository : ICommuneRepository
    {
        private readonly ApplicationDbContext _context;

        public CommuneRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Commune?> GetByIdAsync(Guid id)
        {
            return await _context.Communes
                .Include(c => c.Wilaya)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Commune?> GetByNameAsync(string name)
        {
            return await _context.Communes
                .Include(c => c.Wilaya)
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<List<Commune>> GetByWilayaIdAsync(Guid wilayaId)
        {
            return await _context.Communes
                .Include(c => c.Wilaya)
                .Where(c => c.WilayaId == wilayaId)
                .ToListAsync();
        }

        public async Task<List<Commune>> GetAllAsync()
        {
            return await _context.Communes
                .Include(c => c.Wilaya)
                .ToListAsync();
        }

        public async Task<(List<Commune> Communes, int Total)> GetAllAsync(int page, int pageSize, string? name = null, Guid? wilayaId = null)
        {
            var query = _context.Communes
                .Include(c => c.Wilaya)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            if (wilayaId.HasValue && wilayaId.Value != Guid.Empty)
            {
                query = query.Where(c => c.WilayaId == wilayaId.Value);
            }

            // Get total count
            var total = await query.CountAsync();

            // Apply pagination
            var communes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (communes, total);
        }

        public async Task AddAsync(Commune commune)
        {
            await _context.Communes.AddAsync(commune);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Commune commune)
        {
            _context.Communes.Update(commune);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var commune = await GetByIdAsync(id);
            if (commune != null)
            {
                _context.Communes.Remove(commune);
                await _context.SaveChangesAsync();
            }
        }
    }
}