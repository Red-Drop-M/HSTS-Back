using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Domain.ValueObjects;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class UpdateGlobalStockHandler : IRequestHandler<UpdateGlobalStockCommand, (GlobalStockDTO? globalStock, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<UpdateGlobalStockHandler> _logger;

        public UpdateGlobalStockHandler(
            IGlobalStockRepository globalStockRepository,
            IBloodBagRepository bloodBagRepository,
            ILogger<UpdateGlobalStockHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(GlobalStockDTO? globalStock, BaseException? err)> Handle(UpdateGlobalStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existingStock = await _globalStockRepository.GetByKeyAsync(command.BloodType, command.BloodBagType);
                if (existingStock == null)
                {
                    _logger.LogWarning("Global stock for blood type {BloodType} and bag type {BagType} not found", 
                        command.BloodType.Value, command.BloodBagType.Value);
                    return (null, new NotFoundException("Global stock not found", "update_global_stock"));
                }

                // If any count is not provided, calculate all counts from actual blood bags
                if (!command.ReadyCount.HasValue || !command.CountExpired.HasValue || !command.AcquiredCount.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var sevenDaysFromNow = today.AddDays(7);
                    
                    // Create filter for this blood type and bag type
                    var filter = new BloodBagFilter
                    {
                        BloodType = command.BloodType,
                        BloodBagType = command.BloodBagType,
                        IsDeleted = false // Exclude deleted blood bags
                    };

                    // Get all blood bags of this type
                    var (bloodBags, _) = await _bloodBagRepository.GetAllAsync(1, int.MaxValue, filter);
                    
                    // Calculate counts
                    int readyCount = bloodBags.Count(b => b.Status.Value == "ready");
                    int expiredCount = bloodBags.Count(b => b.Status.Value == "expired");
                    int acquiredCount = bloodBags.Count(b => b.Status.Value == "acquired");
                    int expiringCount = bloodBags.Count(b => 
                        b.Status.Value == "ready" && 
                        b.ExpirationDate.HasValue && 
                        b.ExpirationDate.Value > today && 
                        b.ExpirationDate.Value <= sevenDaysFromNow);

                    _logger.LogInformation("Auto-calculated counts for {BloodType} {BloodBagType}: Ready={Ready}, Acquired={Acquired}, Expired={Expired}, Expiring={Expiring}", 
                        command.BloodType.Value, command.BloodBagType.Value, readyCount, acquiredCount, expiredCount, expiringCount);

                    // Use calculated values for counts not provided in command
                    existingStock.UpdateCounts(
                        command.CountExpired ?? expiredCount,
                        command.CountExpiring ?? expiringCount,
                        command.ReadyCount ?? readyCount,
                        command.AcquiredCount ?? acquiredCount);
                }
                else
                {
                    // All counts provided, use them directly
                    existingStock.UpdateCounts(
                        command.CountExpired.Value,
                        command.CountExpiring ?? existingStock.CountExpiring,
                        command.ReadyCount.Value,
                        command.AcquiredCount.Value);
                }

                // Update min and critical stock if provided
                if (command.MinStock.HasValue || command.CriticalStock.HasValue)
                {
                    existingStock.UpdateThresholds(
                        command.MinStock ?? existingStock.MinStock,
                        command.CriticalStock ?? existingStock.CriticalStock);
                }

                await _globalStockRepository.UpdateAsync(existingStock);
                _logger.LogInformation("Updated global stock for blood type {BloodType} and bag type {BagType}", 
                    existingStock.BloodType.Value, existingStock.BloodBagType.Value);

                return (new GlobalStockDTO
                {
                    BloodType = existingStock.BloodType.Value,
                    BloodBagType = existingStock.BloodBagType.Value,
                    CountExpired = existingStock.CountExpired,
                    CountExpiring = existingStock.CountExpiring,
                    ReadyCount = existingStock.ReadyCount,
                    AcquiredCount = existingStock.AcquiredCount,
                    MinStock = existingStock.MinStock,
                    CriticalStock = existingStock.CriticalStock
                }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error updating global stock");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating global stock");
                return (null, new InternalServerException(ex.Message, "update_global_stock"));
            }
        }
    }
}