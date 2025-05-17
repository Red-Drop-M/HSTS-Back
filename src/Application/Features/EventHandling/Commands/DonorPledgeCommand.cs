using MediatR;
using Domain.Events;
namespace Application.Features.EventHandling.Commands
{
    public class DonorPledgeCommand : IRequest<Unit>
    {
        public DonorPledgeEvent Payload { get; }

        public DonorPledgeCommand(DonorPledgeEvent payload)
        {
            Payload = payload;
        }
    }
}