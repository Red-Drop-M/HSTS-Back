using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class CreateGlobalStockHandler : IRequestHandler<CreateGlobalStockCommand, (GlobalStockDTO? globalStock, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<CreateGlobalStockHandler> _logger;

        public CreateGlobalStockHandler(
            IGlobalStockRepository globalStockRepository,
            ILogger<CreateGlobalStockHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
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

                // Create a new global stock
                var newGlobalStock = new GlobalStock(
                    command.BloodType,
                    command.BloodBagType,
                    command.CountExpired,
                    command.CountExpiring,
                    command.ReadyCount,
                    command.MinStock,
                    command.CriticalStock);

                await _globalStockRepository.AddAsync(newGlobalStock);
                _logger.LogInformation("Created new global stock for blood type {BloodType} and bag type {BagType}", 
                    newGlobalStock.BloodType.Value, newGlobalStock.BloodBagType.Value);

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
                return (null, new InternalServerException("Failed to create global stock", "create_global_stock"));
            }
        }
    }
}