using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;

namespace Application.Features.GlobalStockManagement.Commands
{
    public class DeleteGlobalStockCommand : IRequest<BaseException?>
    {
        public BloodType BloodType { get; }
        public BloodBagType BloodBagType { get; }

        public DeleteGlobalStockCommand(BloodType bloodType, BloodBagType bloodBagType)
        {
            BloodType = bloodType;
            BloodBagType = bloodBagType;
        }
    }
}