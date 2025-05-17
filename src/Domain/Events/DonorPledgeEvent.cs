using Domain.ValueObjects;

namespace Domain.Events
{
    public sealed record DonorPledgeEvent(
        String DonorName,       // Reference by ID
        Guid RequestId,
        DateOnly PledgedAt,
        PledgeStatus Status
    );
}