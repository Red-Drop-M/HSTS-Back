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

        public async Task<GlobalStock?> GetByKeyAsync(BloodType bloodGroup, BloodBagType bloodBagType)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.BloodType == bloodGroup && gs.BloodBagType == bloodBagType);
        }

        public async Task<GlobalStock?> GetByBloodBagTypeAsync(BloodBagType bloodBagType)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.BloodBagType == bloodBagType);
        }

        public async Task<GlobalStock?> GetByBloodGroupAsync(BloodType bloodGroup)
        {
            return await _context.GlobalStocks
                .FirstOrDefaultAsync(gs => gs.BloodType == bloodGroup);
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

        public async Task DeleteAsync(BloodType BT, BloodBagType BBT)
        {
            var globalStock = await GetByKeyAsync(BT, BBT);
            if (globalStock != null)
            {
                _context.GlobalStocks.Remove(globalStock);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<GlobalStock>> GetAllAsync(BloodType? bloodType = null, BloodBagType? bloodBagType = null)
        {
            var query = _context.GlobalStocks.AsQueryable();

            if (bloodType != null)
            {
                query = query.Where(gs => gs.BloodType == bloodType);
            }

            if (bloodBagType != null)
            {
                query = query.Where(gs => gs.BloodBagType == bloodBagType);
            }

            return await query.ToListAsync();
        }
    }
}
