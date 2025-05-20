using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Queries;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class GetGlobalStockByKeyHandler : IRequestHandler<GetGlobalStockByKeyQuery, (GlobalStockDTO? globalStock, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<GetGlobalStockByKeyHandler> _logger;

        public GetGlobalStockByKeyHandler(
            IGlobalStockRepository globalStockRepository,
            ILogger<GetGlobalStockByKeyHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _logger = logger;
        }

        public async Task<(GlobalStockDTO? globalStock, BaseException? err)> Handle(GetGlobalStockByKeyQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var globalStock = await _globalStockRepository.GetByKeyAsync(query.BloodType, query.BloodBagType);

                if (globalStock == null)
                {
                    _logger.LogWarning("Global stock not found for blood type {BloodType} and bag type {BagType}", 
                        query.BloodType.Value, query.BloodBagType.Value);
                    return (null, new NotFoundException("Global stock not found", "get_global_stock_by_key"));
                }

                var stockDto = new GlobalStockDTO
                {
                    BloodType = globalStock.BloodType.Value,
                    BloodBagType = globalStock.BloodBagType.Value,
                    CountExpired = globalStock.CountExpired,
                    CountExpiring = globalStock.CountExpiring,
                    ReadyCount = globalStock.ReadyCount,
                    MinStock = globalStock.MinStock,
                    CriticalStock = globalStock.CriticalStock
                };

                _logger.LogInformation("Retrieved global stock for blood type {BloodType} and bag type {BagType}", 
                    globalStock.BloodType.Value, globalStock.BloodBagType.Value);
                return (stockDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error getting global stock");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting global stock");
                return (null, new InternalServerException("Failed to get global stock", "get_global_stock_by_key"));
            }
        }
    }
}