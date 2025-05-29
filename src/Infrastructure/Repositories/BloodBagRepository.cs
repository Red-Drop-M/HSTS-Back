using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Application.DTOs;

namespace Infrastructure.Repositories
{
    public class BloodBagRepository : IBloodBagRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IRequestRepository _requestRepository;
        private readonly IServiceRepository _serviceRepository;

        public BloodBagRepository(
            ApplicationDbContext context,
            IRequestRepository requestRepository,
            IServiceRepository serviceRepository)
        {
            _context = context;
            _requestRepository = requestRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<BloodBag?> GetByIdAsync(Guid id)
        {
            return await _context.BloodBags
                .Where(b => b.Id == id && !b.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BloodBag?>> GetByBloodGroupAsync(BloodType bloodGroup)
        {
            return await _context.BloodBags
                .Where(b => b.BloodType == bloodGroup && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByBloodBagTypeAsync(BloodBagType bloodBagType)
        {
            return await _context.BloodBags
                .Where(b => b.BloodBagType == bloodBagType && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByExpiryDateAsync(DateOnly expiryDate)
        {
            return await _context.BloodBags
                .Where(b => b.ExpirationDate == expiryDate && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByAcquiredDateAsync(DateOnly acquiredDate)
        {
            return await _context.BloodBags
                .Where(b => b.AcquiredDate == acquiredDate && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByStatusAsync(BloodBagStatus status)
        {
            return await _context.BloodBags
                .Where(b => b.Status == status && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag?>> GetByDonorIdAsync(Guid donorId)
        {
            return await _context.BloodBags
                .Where(b => b.DonorId == donorId && !b.IsDeleted)
                .Cast<BloodBag?>()
                .ToListAsync();
        }

        public async Task<List<BloodBag>> GetByRequestIdAsync(Guid requestId)
        {
            return await _context.BloodBags
                .Where(b => b.RequestId == requestId && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(BloodBag bloodBag)
        {
            await _context.BloodBags.AddAsync(bloodBag);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BloodBag bloodBag)
        {
            _context.BloodBags.Update(bloodBag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var bloodBag = await GetByIdAsync(id);
            if (bloodBag != null)
            {
                bloodBag.MarkAsDeleted();
                _context.BloodBags.Update(bloodBag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<BloodBag>, int)> GetAllAsync(int page, int pageSize, BloodBagFilter filter)
        {
            var query = _context.BloodBags
                .Where(b => !b.IsDeleted)
                .AsQueryable();

            // Handle single blood type filtering
            if (filter.BloodType != null)
                query = query.Where(b => b.BloodType == filter.BloodType);

            // Handle multiple blood types filtering
            if (filter.BloodTypes != null && filter.BloodTypes.Any())
                query = query.Where(b => filter.BloodTypes.Contains(b.BloodType.Value));

            if (filter.BloodBagType != null)
                query = query.Where(b => b.BloodBagType == filter.BloodBagType);

            if (filter.ExpirationDate != null)
                query = query.Where(b => filter.ExpirationDate.HasValue && b.ExpirationDate == filter.ExpirationDate.Value);

            if (filter.AcquiredDate != null)
                query = query.Where(b => b.AcquiredDate == filter.AcquiredDate);

            if (filter.Status != null)
                query = query.Where(b => b.Status == filter.Status);

            if (filter.DonorId != null)
                query = query.Where(b => b.DonorId == filter.DonorId);

            if (filter.RequestId != null)
                query = query.Where(b => b.RequestId == filter.RequestId);
            query = query.OrderBy(b => b.Id);
            var total = await query.CountAsync();

            var bloodBags = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (bloodBags, total);
        }

        public async Task<List<BloodBag>> GetByRequestIdsAsync(List<Guid> requestIds)
        {
            if (requestIds == null || !requestIds.Any())
            {
                return new List<BloodBag>();
            }

            return await _context.BloodBags
                .Where(b => b.RequestId.HasValue && requestIds.Contains(b.RequestId.Value) && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<RequestDto>> GetRequestDtosAsync(int page, int pageSize, BloodBagFilter filter)
        {
            var requestDtos = new List<RequestDto>();

            // 1. Fetch all blood requests matching the filter
            var requestFilter = new RequestFilter();
            
            // Map BloodBagFilter to RequestFilter
            if (filter.BloodType != null)
                requestFilter.BloodType = filter.BloodType.Value;
                
            if (filter.BloodBagType != null)
                requestFilter.BloodBagType = filter.BloodBagType.Value;
            
            if (filter.RequestId != null)
                requestFilter.RequestId = filter.RequestId;

            var (requests, total) = await _requestRepository.GetAllAsync(page, pageSize, requestFilter);

            // 2. Process each request individually
            foreach (var request in requests)
            {
                // Create base DTO with request data
                var requestDto = new RequestDto
                {
                    Id = request.Id,
                    Priority = request.Priority.Value,
                    BloodType = request.BloodType.Value,
                    BloodBagType = request.BloodBagType.Value,
                    RequestDate = request.RequestDate,
                    DueDate = request.DueDate,
                    Status = request.Status.Value,
                    MoreDetails = request.MoreDetails,
                    RequiredQty = request.RequiredQty,
                    AquiredQty = request.AquiredQty,
                    ServiceId = request.ServiceId,
                    DonorId = request.DonorId,
                    BloodBags = new List<BloodBagDTO>() // Initialize empty list
                };
                
                // 3. If request has a ServiceId, fetch and add the service
                if (request.ServiceId.HasValue)
                {
                    var service = await _serviceRepository.GetByIdAsync(request.ServiceId.Value);
                    if (service != null)
                    {
                        requestDto.Service = service.Name;
                    }
                }
                
                // 4. Fetch blood bags for this request, if any
                var bloodBags = await GetByRequestIdAsync(request.Id);
                if (bloodBags != null && bloodBags.Any())
                {
                    requestDto.BloodBags = bloodBags.Select(b => new BloodBagDTO
                    {
                        Id = b.Id,
                        BloodType = b.BloodType,
                        BloodBagType = b.BloodBagType,
                        Status = b.Status,
                        ExpirationDate = b.ExpirationDate,
                        AcquiredDate = b.AcquiredDate,
                        DonorId = b.DonorId ?? Guid.Empty,
                        RequestId = b.RequestId
                    }).ToList();
                }
                
                requestDtos.Add(requestDto);
            }

            return requestDtos;
        }
    }
}
