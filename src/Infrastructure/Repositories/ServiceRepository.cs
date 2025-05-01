using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;


namespace Infrastructure.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Service?> GetByNameAsync(string name)
        {
            return await _context.Services
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var service = await GetByIdAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Service?>> GetServicesAsync()
        {
            return await _context.Services
                .Cast<Service?>()
                .ToListAsync();
        }

        public async Task<(List<Service>, int)> GetAllAsync(int page, int pageSize, ServiceFilter filter)
        {
            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(s => s.Name.Contains(filter.Name));

            var total = await query.CountAsync();

            var services = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (services, total);
        }

        
    }
}
