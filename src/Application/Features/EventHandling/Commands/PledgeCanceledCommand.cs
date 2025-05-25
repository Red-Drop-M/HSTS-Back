using MediatR;
using Domain.Events;

namespace Application.Features.EventHandling.Commands
{
    public class PledgeCanceledCommand : IRequest
    {
        public PledgeCanceledEvent Payload { get; }

        public PledgeCanceledCommand(PledgeCanceledEvent payload)
        {
            Payload = payload;
        }
    }
}