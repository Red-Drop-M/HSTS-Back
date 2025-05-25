using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BloodTransferCenterRepository : IBloodTransferCenterRepository
    {
        private readonly ApplicationDbContext _context;
        
        public BloodTransferCenterRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<BloodTransferCenter?> GetByIdAsync(Guid id)
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .FirstOrDefaultAsync(btc => btc.Id == id);
        }
        
        public async Task<BloodTransferCenter?> GetByNameAsync(string name)
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .FirstOrDefaultAsync(btc => btc.Name == name);
        }
        
        public async Task<BloodTransferCenter?> GetByEmailAsync(string email)
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .FirstOrDefaultAsync(btc => btc.Email == email);
        }
        
        public async Task<List<BloodTransferCenter>> GetByWilayaIdAsync(Guid wilayaId)
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .Where(btc => btc.WilayaId == wilayaId)
                .ToListAsync();
        }
        
        public async Task<List<BloodTransferCenter>> GetAllAsync()
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .ToListAsync();
        }
        
        public async Task<(List<BloodTransferCenter> Centers, int Total)> GetAllAsync(int page, int pageSize)
        {
            var query = _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .AsQueryable();
                
            var total = await query.CountAsync();
            
            var centers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (centers, total);
        }
        
        // Add this method to handle setting a center as primary
        public async Task SetAsPrimaryAsync(Guid id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // First, set all centers to not primary
                foreach (var center in _context.BloodTransferCenters)
                {
                    center.UnsetPrimary();
                }
                
                // Now set the selected one as primary
                var primaryCenter = await _context.BloodTransferCenters
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (primaryCenter != null)
                {
                    primaryCenter.SetAsPrimary();
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        // Get the current primary center
        public async Task<BloodTransferCenter?> GetPrimaryAsync()
        {
            return await _context.BloodTransferCenters
                .Include(btc => btc.Wilaya)
                .FirstOrDefaultAsync(btc => btc.IsPrimary);
        }
        
        // Override the Add method to enforce singleton if needed
        public async Task AddAsync(BloodTransferCenter center)
        {
            // If this is the only center, make it primary
            bool hasCenters = await _context.BloodTransferCenters.AnyAsync();
            
            if (!hasCenters)
            {
                center.SetAsPrimary();
            }
            
            await _context.BloodTransferCenters.AddAsync(center);
            await _context.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(BloodTransferCenter center)
        {
            _context.BloodTransferCenters.Update(center);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(Guid id)
        {
            var center = await GetByIdAsync(id);
            if (center != null)
            {
                _context.BloodTransferCenters.Remove(center);
                await _context.SaveChangesAsync();
            }
        }
    }
}