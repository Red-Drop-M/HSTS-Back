using MediatR;
using Domain.ValueObjects;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.PledgeManagement.Commands
{
    public class UpdatePledgeStatusCommand : IRequest<(DonorPledgeDTO pledge, BaseException? error)>
    {
        public Guid DonorId { get; }
        public Guid RequestId { get; }
        public PledgeStatus Status { get; }

        public UpdatePledgeStatusCommand(Guid donorId, Guid requestId, PledgeStatus status)
        {
            DonorId = donorId;
            RequestId = requestId;
            Status = status;
        }
    }
}