using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class GlobalStockRepository : IGlobalStockRepository
    {
        private readonly ApplicationDbContext _context;

        public GlobalStockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GlobalStock?> GetByIdAsync(Guid id)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task<GlobalStock?> GetByBloodBagTypeAsync(BloodBagType bloodBagType)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.BloodBagType == bloodBagType);
        }

        public async Task<GlobalStock?> GetByBloodGroupAsync(BloodType bloodGroup)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.BloodGroup == bloodGroup);
        }

        public async Task AddAsync(GlobalStock globalStock)
        {
            await _context.GlobalStocks.AddAsync(globalStock);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GlobalStock globalStock)
        {
            _context.GlobalStocks.Update(globalStock);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var globalStock = await GetByIdAsync(id);
            if (globalStock != null)
            {
                _context.GlobalStocks.Remove(globalStock);
                await _context.SaveChangesAsync();
            }
        }
    }
}
