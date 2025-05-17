using Domain.ValueObjects;

namespace Domain.Events
{
    public sealed record DonorPledgeEvent(
        DonorData Donor,      // Reference by ID
        Guid RequestId,
        DateOnly PledgedAt,
        PledgeStatus Status
    );
    public sealed record DonorData(
        string DonorName,
        string Email,
        string PhoneNumber,
        string Address,
        string NIN,
        DateOnly DateOfBirth,
        DateOnly LastDonationDate
    );
}