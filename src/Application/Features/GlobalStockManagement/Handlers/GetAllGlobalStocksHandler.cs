using MediatR;
using Domain.Repositories;
using Domain.ValueObjects;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Queries;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Features.GlobalStockManagement.Handlers
{
    public class GetAllGlobalStocksHandler : IRequestHandler<GetAllGlobalStocksQuery, (List<GlobalStockDTO>? stocks, int? total, BaseException? err)>
    {
        private readonly IGlobalStockRepository _globalStockRepository;
        private readonly ILogger<GetAllGlobalStocksHandler> _logger;

        public GetAllGlobalStocksHandler(
            IGlobalStockRepository globalStockRepository,
            ILogger<GetAllGlobalStocksHandler> logger)
        {
            _globalStockRepository = globalStockRepository;
            _logger = logger;
        }

        public async Task<(List<GlobalStockDTO>? stocks, int? total, BaseException? err)> Handle(GetAllGlobalStocksQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var stocks = await _globalStockRepository.GetAllAsync(
                    query.BloodType != null ? BloodType.FromString(query.BloodType) : null,
                    query.BloodBagType != null ? BloodBagType.Convert(query.BloodBagType) : null);

                if (stocks == null || !stocks.Any())
                {
                    _logger.LogWarning("No global stocks found");
                    return (null, null, new NotFoundException("No global stocks found", "get_all_global_stocks"));
                }

                // Apply critical filter if provided
                if (query.Critical.HasValue && query.Critical.Value)
                {
                    stocks = stocks.Where(s => s.ReadyCount <= s.CriticalStock).ToList();
                    if (stocks.Count == 0)
                    {
                        _logger.LogWarning("No critical global stocks found");
                        return (null, null, new NotFoundException("No critical global stocks found", "get_all_global_stocks"));
                    }
                }

                var stockDtos = stocks.Select(s => new GlobalStockDTO
                {
                    BloodType = s.BloodType.Value,
                    BloodBagType = s.BloodBagType.Value,
                    CountExpired = s.CountExpired,
                    CountExpiring = s.CountExpiring,
                    ReadyCount = s.ReadyCount,
                    MinStock = s.MinStock,
                    CriticalStock = s.CriticalStock
                }).ToList();

                _logger.LogInformation("Retrieved {Count} global stocks", stockDtos.Count);
                return (stockDtos, stockDtos.Count, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error getting global stocks");
                return (null, null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting global stocks");
                return (null, null, new InternalServerException("Failed to get global stocks", "get_all_global_stocks"));
            }
        }
    }
}