using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Application.Features.BloodBagManagement.Commands;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class UpdateBloodBagHandler : IRequestHandler<UpdateBloodBagCommand, (BloodBagDTO? bloodBag, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<UpdateBloodBagHandler> _logger;
        private readonly ApplicationDbContext _context; // Use your specific DbContext type

        public UpdateBloodBagHandler(
            IBloodBagRepository bloodBagRepository, 
            IGlobalStockRepository globalStockRepository,
            ILogger<UpdateBloodBagHandler> logger,
            ApplicationDbContext context) // Change parameter type
        {
            _bloodBagRepository = bloodBagRepository;
            _globalStockRepository = globalStockRepository;
            _logger = logger;
            _context = context;
        }

        public async Task<(BloodBagDTO? bloodBag, BaseException? err)> Handle(UpdateBloodBagCommand command, CancellationToken ct)
        {
            try
            {
                var bloodBag = await _bloodBagRepository.GetByIdAsync(command.Id);
                if (bloodBag == null)
                {
                    _logger.LogError("Blood bag not found");
                    return (null, new NotFoundException("No blood bag found with the provided ID", "Updating blood bag"));
                }

                // Store original status for comparison
                var originalStatus = bloodBag.Status;
                
                // Update blood bag details
                bloodBag.UpdateDetails(command.BloodBagType, command.BloodType, command.ExpirationDate, command.AcquiredDate, command.RequestId);
                
                // Check if status change is needed
                bool statusChanged = false;
                if (command.Status != null && originalStatus.Value != command.Status.Value)
                {
                    bloodBag.UpdateStatus(command.Status);
                    statusChanged = true;
                    _logger.LogInformation("Status changed from {OldStatus} to {NewStatus}", 
                        originalStatus.Value, command.Status.Value);
                }
                
                await _bloodBagRepository.UpdateAsync(bloodBag);

                // If status changed, update global stock counts
                if (statusChanged)
                {
                    await UpdateGlobalStockForStatusChange(
                        originalStatus, 
                        bloodBag.Status, 
                        bloodBag.BloodType, 
                        bloodBag.BloodBagType);
                }

                _logger.LogInformation("Blood bag updated successfully");

                var bloodBagDto = new BloodBagDTO
                {
                    Id = bloodBag.Id,
                    BloodType = bloodBag.BloodType,
                    BloodBagType = bloodBag.BloodBagType,
                    ExpirationDate = bloodBag.ExpirationDate,
                    AcquiredDate = bloodBag.AcquiredDate,
                    Status = bloodBag.Status,
                    DonorId = bloodBag.DonorId ?? Guid.Empty,
                    RequestId = bloodBag.RequestId
                };

                return (bloodBagDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError("Error while updating blood bag: {Message}", ex.Message);
                return (null, ex);
            }
        }
        
        private async Task UpdateGlobalStockForStatusChange(
            BloodBagStatus oldStatus, 
            BloodBagStatus newStatus, 
            BloodType bloodType, 
            BloodBagType bloodBagType)
        {
            try
            {
                var globalStock = await _globalStockRepository.GetByKeyAsync(bloodType, bloodBagType);
                if (globalStock == null)
                {
                    _logger.LogWarning("No global stock found for {BloodType} {BloodBagType} - creating new stock", 
                        bloodType.Value, bloodBagType.Value);
                        
                    // Create a new global stock if none exists
                    globalStock = new GlobalStock(
                        bloodType, 
                        bloodBagType,
                        0, // expired
                        0, // expiring
                        0, // ready
                        0, // acquired
                        5, // min stock (default)
                        3  // critical stock (default)
                    );
                    
                    await _globalStockRepository.AddAsync(globalStock);
                }
                
                _logger.LogInformation("Before update - Global stock for {BloodType} {BloodBagType}: Ready={Ready}, Acquired={Acquired}", 
                    bloodType.Value, bloodBagType.Value, globalStock.ReadyCount, globalStock.AcquiredCount);
                
                // Get all blood bags of this type and blood group
                var filter = new BloodBagFilter
                {
                    BloodType = bloodType,
                    BloodBagType = bloodBagType,
                    IsDeleted = false // Exclude deleted blood bags
                };
                
                var (bloodBags, _) = await _bloodBagRepository.GetAllAsync(1, int.MaxValue, filter);

                // Log all blood bags for detailed debugging
                _logger.LogInformation("Found {Count} blood bags for {BloodType} {BloodBagType}", 
                    bloodBags.Count, bloodType.Value, bloodBagType.Value);
                foreach (var bag in bloodBags)
                {
                    _logger.LogInformation("Blood bag ID: {Id}, Status: '{Status}'", 
                        bag.Id, bag.Status.Value);
                }
                
                // Get counts for each status
                int readyCount = bloodBags.Count(b => b.Status.Value == "ready");
                int acquiredCount = bloodBags.Count(b => 
                    b.Status.Value.ToLower() == "acquired" || 
                    b.Status.Value.ToLower() == "aquired");
                
                // Calculate expiring and expired counts
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysFromNow = today.AddDays(7);
                int expiringCount = bloodBags.Count(b => 
                    b.Status.Value == "ready" && 
                    b.ExpirationDate.HasValue && 
                    b.ExpirationDate.Value > today && 
                    b.ExpirationDate.Value <= sevenDaysFromNow);
                    
                int expiredCount = bloodBags.Count(b => b.Status.Value == "expired");
                
                _logger.LogInformation(
                    "Recalculated counts: Ready={Ready}, Acquired={Acquired}, Expiring={Expiring}, Expired={Expired}", 
                    readyCount, acquiredCount, expiringCount, expiredCount);
                
                // Update global stock with the correct counts
                globalStock.UpdateCounts(expiredCount, expiringCount, readyCount, acquiredCount);
                
                // Force entity state to be tracked
                _context.Entry(globalStock).State = EntityState.Modified;
                
                // Save changes to database and log results
                await _globalStockRepository.UpdateAsync(globalStock);
                
                // Explicitly call SaveChanges to ensure changes are persisted
                await _context.SaveChangesAsync();
                
                // Verify by fetching fresh data
                var updatedStock = await _globalStockRepository.GetByKeyAsync(bloodType, bloodBagType);
                _logger.LogInformation("After update: ReadyCount={Ready}, AcquiredCount={Acquired}", 
                    updatedStock?.ReadyCount ?? -1, updatedStock?.AcquiredCount ?? -1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update global stock for status change");
            }
        }
    }
}