using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public RequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Request?> GetByIdAsync(Guid id)
        {
            return await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Request?> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.Requests
                .FirstOrDefaultAsync(r => r.ServiceId == serviceId);
        }

        public async Task<List<Request>> GetByStatusAsync(RequestStatus status)
        {
            return await _context.Requests
                .Where(r => r.Status == status)
                .ToListAsync();
        }

        public async Task<Request> AddAsync(Request request)
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request; // Return the added Request object
        }

        public async Task UpdateAsync(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var request = await GetByIdAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Request>> GetByBloodBagTypeAsync(BloodBagType bloodBagType)
        {
            return await _context.Requests
                .Where(r => r.BloodBagType == bloodBagType)
                .ToListAsync();
        }

        public async Task<List<Request>> GetByPriorityAsync(Priority priority)
        {
            return await _context.Requests
                .Where(r => r.Priority == priority)
                .ToListAsync();
        }

        public async Task<List<Request>> GetByRequestDateAsync(DateOnly requestDate)
        {
            return await _context.Requests
                .Where(r => r.RequestDate == requestDate)
                .ToListAsync();
        }

        public async Task<List<Request>> GetByDueDateAsync(DateOnly dueDate)
        {
            return await _context.Requests
                .Where(r => r.DueDate == dueDate)
                .ToListAsync();
        }

        public async Task<List<Request>> GetAllAsync()
        {
            return await _context.Requests
                .ToListAsync();
        }

        public async Task<List<Request>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Requests
                .Where(r => r.DonorId == donorId)
                .ToListAsync();
        }
    }
}