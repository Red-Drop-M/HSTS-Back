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
                .Where(r => r.Id == id && !r.IsDeleted)
                .FirstOrDefaultAsync();
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

        public async Task AddAsync(Request request)
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
             // Return the added Request object
        }

        public async Task UpdateAsync(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                request.MarkAsDeleted();
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
            return await _context.Requests.Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task<List<Request>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.Requests
                .Where(r => r.DonorId == donorId)
                .ToListAsync();
        }
        public async Task<(List<Request>, int)> GetAllAsync(int page, int pageSize, RequestFilter filter)
        {
            // Start with a base query that only filters by IsDeleted
            var query = _context.Requests
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            // Apply filters for scalar properties - ServiceId filter
            if (filter.ServiceId.HasValue)
            {
                query = query.Where(r => r.ServiceId == filter.ServiceId);
            }

            // DonorId filter
            if (filter.DonorId.HasValue)
            {
                query = query.Where(r => r.DonorId == filter.DonorId);
            }

            // Order by Id to ensure consistent ordering
            query = query.OrderBy(r => r.Id);

            // Get all requests from the database that match the basic filters
            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Apply value object filters in memory
            var filteredRequests = requests;

            if (!string.IsNullOrEmpty(filter.Status))
            {
                filteredRequests = filteredRequests.Where(r => r.Status.Value == filter.Status).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Priority))
            {
                filteredRequests = filteredRequests.Where(r => r.Priority.Value == filter.Priority).ToList();
            }

            if (!string.IsNullOrEmpty(filter.BloodType))
            {
                filteredRequests = filteredRequests.Where(r => r.BloodType.Value == filter.BloodType).ToList();
            }

            if (!string.IsNullOrEmpty(filter.BloodBagType))
            {
                filteredRequests = filteredRequests.Where(r => r.BloodBagType.Value == filter.BloodBagType).ToList();
            }

            // Calculate the total count for all matching records (including those beyond the current page)
            var totalFiltered = await ApplyFilterCountAsync(query, filter);

            return (filteredRequests, totalFiltered);
        }

        private async Task<int> ApplyFilterCountAsync(IQueryable<Request> baseQuery, RequestFilter filter)
        {
            // Get all requests that match the database-level filters
            var allMatchingRequests = await baseQuery.ToListAsync();
            
            // Apply the same in-memory filters used in GetAllAsync to get an accurate count
            if (!string.IsNullOrEmpty(filter.Status))
            {
                allMatchingRequests = allMatchingRequests.Where(r => r.Status.Value == filter.Status).ToList();
            }

            if (!string.IsNullOrEmpty(filter.Priority))
            {
                allMatchingRequests = allMatchingRequests.Where(r => r.Priority.Value == filter.Priority).ToList();
            }

            if (!string.IsNullOrEmpty(filter.BloodType))
            {
                allMatchingRequests = allMatchingRequests.Where(r => r.BloodType.Value == filter.BloodType).ToList();
            }

            if (!string.IsNullOrEmpty(filter.BloodBagType))
            {
                allMatchingRequests = allMatchingRequests.Where(r => r.BloodBagType.Value == filter.BloodBagType).ToList();
            }

            return allMatchingRequests.Count;
        }

        public async Task UpdateWithoutNavigationAsync(Request request)
        {
            // Attach the entity but only mark specific properties as modified
            _context.Attach(request);
            
            // Mark only the scalar properties you want to update as Modified
            _context.Entry(request).Property(x => x.Status).IsModified = true;
            _context.Entry(request).Property(x => x.AquiredQty).IsModified = true;
            _context.Entry(request).Property(x => x.RequiredQty).IsModified = true;
            
            // Explicitly ignore navigation properties
            _context.Entry(request).Reference(r => r.Service).IsModified = false;
            _context.Entry(request).Reference(r => r.Donor).IsModified = false;
            _context.Entry(request).Collection(r => r.BloodSacs).IsModified = false;
            _context.Entry(request).Collection(r => r.Pledges).IsModified = false;
            
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<Request> requests)
        {
            // First clear the change tracker to avoid entity tracking conflicts
            _context.ChangeTracker.Clear();
            
            foreach (var request in requests)
            {
                // Attach each entity but only update specific properties
                _context.Attach(request);
                
                // Mark properties for update
                _context.Entry(request).Property(x => x.Status).IsModified = true;
                
                // Explicitly ignore navigation properties to avoid tracking conflicts
                _context.Entry(request).Reference(r => r.Service).IsModified = false;
                _context.Entry(request).Reference(r => r.Donor).IsModified = false;
                _context.Entry(request).Collection(r => r.BloodSacs).IsModified = false;
                _context.Entry(request).Collection(r => r.Pledges).IsModified = false;
            }
            
            await _context.SaveChangesAsync();
        }
    }
}