using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Domain.ValueObjects;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class CreateGlobalStockHandler : IRequestHandler<CreateGlobalStockCommand, (GlobalStockDTO? globalStock, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CreateGlobalStockHandler> _logger;

        public CreateGlobalStockHandler(
            IGlobalStockRepository globalStockRepository,
            IBloodBagRepository bloodBagRepository,
            ILogger<CreateGlobalStockHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(GlobalStockDTO? globalStock, BaseException? err)> Handle(CreateGlobalStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Check if a global stock already exists for this blood type and bag type
                var existingStock = await _globalStockRepository.GetByKeyAsync(
                    command.BloodType, command.BloodBagType);
                
                if (existingStock != null)
                {
                    _logger.LogWarning("Global stock for blood type {BloodType} and bag type {BagType} already exists", 
                        command.BloodType.Value, command.BloodBagType.Value);
                    return (null, new ConflictException("Global stock already exists for this blood type and bag type", "create_global_stock"));
                }

                // Calculate counts from actual blood bags
                var today = DateOnly.FromDateTime(DateTime.Now);
                var sevenDaysFromNow = today.AddDays(7);
                
                // Create filter for this blood type and bag type
                var filter = new BloodBagFilter
                {
                    BloodType = command.BloodType,
                    BloodBagType = command.BloodBagType
                };

                // Get all blood bags of this type
                var (bloodBags, _) = await _bloodBagRepository.GetAllAsync(1, int.MaxValue, filter);
                
                // Calculate counts
                int readyCount = bloodBags.Count(b => b.Status.Value == "ready");
                int expiredCount = bloodBags.Count(b => b.Status.Value == "expired");
                int acquiredCount = bloodBags.Count(b => b.Status.Value == "aquired");
                int expiringCount = bloodBags.Count(b => 
                    b.Status.Value == "ready" && 
                    b.ExpirationDate.HasValue && 
                    b.ExpirationDate.Value > today && 
                    b.ExpirationDate.Value <= sevenDaysFromNow);

                _logger.LogInformation("Calculated counts for {BloodType} {BloodBagType}: Ready={Ready}, Acquired={Acquired}, Expired={Expired}, Expiring={Expiring}", 
                    command.BloodType.Value, command.BloodBagType.Value, readyCount, acquiredCount, expiredCount, expiringCount);

                // Create a new global stock with calculated counts
                var newGlobalStock = new GlobalStock(
                    command.BloodType,
                    command.BloodBagType,
                    expiredCount,
                    expiringCount,
                    readyCount,
                    acquiredCount,
                    command.MinStock,
                    command.CriticalStock);

                await _globalStockRepository.AddAsync(newGlobalStock);
                _logger.LogInformation("Created new global stock for blood type {BloodType} and bag type {BagType} with Ready={Ready}", 
                    newGlobalStock.BloodType.Value, newGlobalStock.BloodBagType.Value, readyCount);

                return (new GlobalStockDTO
                {
                    BloodType = newGlobalStock.BloodType.Value,
                    BloodBagType = newGlobalStock.BloodBagType.Value,
                    CountExpired = newGlobalStock.CountExpired,
                    CountExpiring = newGlobalStock.CountExpiring,
                    ReadyCount = newGlobalStock.ReadyCount,
                    MinStock = newGlobalStock.MinStock,
                    CriticalStock = newGlobalStock.CriticalStock
                }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating global stock");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating global stock");
                return (null, new InternalServerException(ex.Message, "create_global_stock"));
            }
        }
    }
}