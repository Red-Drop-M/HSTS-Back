using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.GlobalStockManagement.Queries
{
    public record GetGlobalStockByKeyQuery(
        BloodType BloodType,
        BloodBagType BloodBagType
    ) : IRequest<(GlobalStockDTO? globalStock, BaseException? err)>;
}