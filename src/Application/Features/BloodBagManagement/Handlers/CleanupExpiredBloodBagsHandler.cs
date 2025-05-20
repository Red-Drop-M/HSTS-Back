using Domain.ValueObjects;
using MediatR;
using Application.Features.BloodBagManagement.Commands;
using Microsoft.Extensions.Logging;
using Domain.Repositories;
using Domain.Entities;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class CleanupExpiredBloodBagsHandler : IRequestHandler<CleanupExpiredBloodBagsCommand>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CleanupExpiredBloodBagsHandler> _logger;
        private readonly IGlobalStockRepository _globalStockRepository;
        
        public CleanupExpiredBloodBagsHandler(
            IBloodBagRepository bloodBagRepository, 
            ILogger<CleanupExpiredBloodBagsHandler> logger,
            IGlobalStockRepository globalStockRepository)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
            _globalStockRepository = globalStockRepository;
        }
        
        public async Task<Unit> Handle(CleanupExpiredBloodBagsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting expired blood bags cleanup process");
            
            // Use a filter to get all non-expired blood bags
            var filter = new BloodBagFilter
            {
                Status = BloodBagStatus.Ready()
            };
            
            // Get all blood bags with a status that is not expired
            var (bloodBags, _) = await _bloodBagRepository.GetAllAsync(1, int.MaxValue, filter);
            
            _logger.LogInformation("Retrieved {Count} blood bags for expiration check", bloodBags.Count);
            
            if (!bloodBags.Any())
            {
                _logger.LogInformation("No non-expired blood bags found to check");
                return Unit.Value;
            }

            // Group blood bags by blood type AND blood bag type
            var groupedBloodBags = bloodBags
                .GroupBy(b => new { BloodType = b.BloodType, BloodBagType = b.BloodBagType })
                .ToDictionary(
                    g => g.Key, 
                    g => g.ToList()
                );

            _logger.LogInformation("Grouped blood bags into {Count} type combinations", groupedBloodBags.Count);

            // Track changes for global stock update
            var expiredCounts = new Dictionary<(BloodType, BloodBagType), int>();

            // Current date for comparison
            var today = DateOnly.FromDateTime(DateTime.Now);
            _logger.LogInformation("Checking expiration against current date: {Today}", today);

            // Process each group
            foreach (var group in groupedBloodBags)
            {
                var bloodType = group.Key.BloodType;
                var bloodBagType = group.Key.BloodBagType;
                var count = 0;

                _logger.LogInformation("Processing group: BloodType={BloodType}, BloodBagType={BloodBagType}, Count={Count}", 
                    bloodType, bloodBagType, group.Value.Count);

                foreach (var bloodBag in group.Value)
                {
                    // Check if the blood bag has an expiration date and if it's expired
                    if (bloodBag.ExpirationDate.HasValue && bloodBag.ExpirationDate.Value < today)
                    {
                        // Only update if it's not already expired
                        if (bloodBag.Status.Value != BloodBagStatus.Expired().Value)
                        {
                            _logger.LogInformation("Blood bag {Id} is expired. Expiration date: {ExpirationDate}, Current date: {Today}", 
                                bloodBag.Id, bloodBag.ExpirationDate, today);
                            
                            // Update the status to expired
                            bloodBag.UpdateStatus(BloodBagStatus.Expired());
                            await _bloodBagRepository.UpdateAsync(bloodBag);
                            count++;
                            
                            _logger.LogInformation("Blood bag {Id} marked as expired", bloodBag.Id);
                        }
                    }
                }

                if (count > 0)
                {
                    expiredCounts[(bloodType, bloodBagType)] = count;
                    _logger.LogInformation("Marked {Count} {BloodType} {BloodBagType} blood bags as expired", 
                        count, bloodType, bloodBagType);
                }
            }

            // Update global stock for each affected blood type and bag type
            foreach (var item in expiredCounts)
            {
                var bloodType = item.Key.Item1;
                var bloodBagType = item.Key.Item2;
                var count = item.Value;

                _logger.LogInformation("Updating global stock for {BloodType} {BloodBagType}, decreasing by {Count}", 
                    bloodType, bloodBagType, count);

                var globalStock = await _globalStockRepository.GetByKeyAsync(
                    bloodType, bloodBagType);
                    
                if (globalStock != null)
                {
                    globalStock.DecrementAvailableCount(count);
                    await _globalStockRepository.UpdateAsync(globalStock);

                    // Check stock levels and log warnings
                    if (globalStock.IsCritical())
                    {
                        _logger.LogWarning("CRITICAL: Stock for {BloodType} {BloodBagType} is critically low at {Count}", 
                            bloodType, bloodBagType, globalStock.ReadyCount);
                    }
                    else if (globalStock.IsMinimal())
                    {
                        _logger.LogWarning("MINIMAL: Stock for {BloodType} {BloodBagType} is minimal at {Count}", 
                            bloodType, bloodBagType, globalStock.ReadyCount);
                    }
                }
            }

            _logger.LogInformation("Blood bag expiration check completed. Updated {TotalCount} blood bags",
                expiredCounts.Values.Sum());
                
            return Unit.Value;
        }
    }
}