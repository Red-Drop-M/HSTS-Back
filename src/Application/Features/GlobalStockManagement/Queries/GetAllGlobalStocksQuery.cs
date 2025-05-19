using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.GlobalStockManagement.Queries
{
    public record GetAllGlobalStocksQuery(
        string? BloodType = null,
        string? BloodBagType = null,
        bool? Critical = null
    ) : IRequest<(List<GlobalStockDTO>? stocks, int? total, BaseException? err)>;
}