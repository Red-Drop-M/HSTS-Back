using Domain.ValueObjects;
namespace Domain.Events
{
    public sealed record DonorPledgeEvent
    (
        Guid DonorId,
        Guid RequestId,
        DateOnly PledgedAt,
        PledgeStatus Status
    );
}