using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.GlobalStockManagement.Commands
{
    public class UpdateGlobalStockCommand : IRequest<(GlobalStockDTO? globalStock, BaseException? err)>
    {
        public BloodType BloodType { get; }
        public BloodBagType BloodBagType { get; }
        public int? CountExpired { get; }
        public int? CountExpiring { get; }
        public int? ReadyCount { get; }
        public int? AcquiredCount { get; } // New property
        public int? MinStock { get; }
        public int? CriticalStock { get; }

        public UpdateGlobalStockCommand(
            BloodType bloodType,
            BloodBagType bloodBagType,
            int? countExpired = null,
            int? countExpiring = null,
            int? readyCount = null,
            int? acquiredCount = null,
            int? minStock = null,
            int? criticalStock = null)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
            CountExpired = countExpired;
            CountExpiring = countExpiring;
            ReadyCount = readyCount;
            AcquiredCount = acquiredCount;
            MinStock = minStock;
            CriticalStock = criticalStock;
        }
    }
}