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

    }
}