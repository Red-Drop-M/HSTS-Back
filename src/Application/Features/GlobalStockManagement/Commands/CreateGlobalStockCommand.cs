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
        public int CountExpired { get; }
        public int CountExpiring { get; }
        public int ReadyCount { get; }
        public int MinStock { get; }
        public int CriticalStock { get; }

        public CreateGlobalStockCommand(
            BloodType bloodType,
            BloodBagType bloodBagType,
            int countExpired,
            int countExpiring,
            int readyCount,
            int minStock,
            int criticalStock)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            CountExpired = countExpired;
            CountExpiring = countExpiring;
            ReadyCount = readyCount;
            MinStock = minStock;
            CriticalStock = criticalStock;
        }
    }
}