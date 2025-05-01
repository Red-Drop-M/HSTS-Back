using Domain.ValueObjects;
namespace Application.DTOs
{
    public class DonorPledgeDTO
    {
        public Guid DonorId { get; set; }
        public Guid RequestId { get; set; }
        public DateOnly PledgedAt { get; set; }
        public PledgeStatus Status { get; set;} = PledgeStatus.Pledged;
    }
}