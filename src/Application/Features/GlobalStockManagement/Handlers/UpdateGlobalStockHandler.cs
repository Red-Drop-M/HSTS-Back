using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class UpdateGlobalStockHandler : IRequestHandler<UpdateGlobalStockCommand, (GlobalStockDTO? globalStock, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<UpdateGlobalStockHandler> _logger;

        public UpdateGlobalStockHandler(
            IGlobalStockRepository globalStockRepository,
            ILogger<UpdateGlobalStockHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _logger = logger;
        }

        public async Task<(GlobalStockDTO? globalStock, BaseException? err)> Handle(UpdateGlobalStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var globalStock = await _globalStockRepository.GetByKeyAsync(
                    command.BloodType, command.BloodBagType);

                if (globalStock == null)
                {
                    _logger.LogWarning("Global stock not found for blood type {BloodType} and bag type {BagType}", 
                        command.BloodType.Value, command.BloodBagType.Value);
                    return (null, new NotFoundException("Global stock not found", "update_global_stock"));
                }

                // Update counts if provided
                if (command.CountExpired.HasValue || command.CountExpiring.HasValue || command.ReadyCount.HasValue)
                {
                    globalStock.UpdateCounts(
                        command.CountExpired ?? globalStock.CountExpired,
                        command.CountExpiring ?? globalStock.CountExpiring,
                        command.ReadyCount ?? globalStock.ReadyCount);
                }

                // Update thresholds if provided
                if (command.MinStock.HasValue || command.CriticalStock.HasValue)
                {
                    globalStock.UpdateThresholds(
                        command.MinStock ?? globalStock.MinStock,
                        command.CriticalStock ?? globalStock.CriticalStock);
                }

                await _globalStockRepository.UpdateAsync(globalStock);
                _logger.LogInformation("Updated global stock for blood type {BloodType} and bag type {BagType}", 
                    globalStock.BloodType.Value, globalStock.BloodBagType.Value);

                return (new GlobalStockDTO
                {
                    BloodType = globalStock.BloodType.Value,
                    BloodBagType = globalStock.BloodBagType.Value,
                    CountExpired = globalStock.CountExpired,
                    CountExpiring = globalStock.CountExpiring,
                    ReadyCount = globalStock.ReadyCount,
                    MinStock = globalStock.MinStock,
                    CriticalStock = globalStock.CriticalStock
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
                return (null, new InternalServerException("Failed to update global stock", "update_global_stock"));
            }
        }
    }
}