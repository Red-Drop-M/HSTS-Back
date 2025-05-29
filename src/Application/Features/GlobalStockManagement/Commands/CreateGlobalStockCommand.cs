using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.GlobalStockManagement.Commands
{
    public class CreateGlobalStockCommand : IRequest<(GlobalStockDTO? globalStock, BaseException? err)>
    {
        public BloodType BloodType { get; }
        public BloodBagType BloodBagType { get; }
        public int MinStock { get; }
        public int CriticalStock { get; }

        public CreateGlobalStockCommand(
            BloodType bloodType,
            BloodBagType bloodBagType,
            int minStock,
            int criticalStock)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            MinStock = minStock;
            CriticalStock = criticalStock;
        }
    }
}