namespace Domain.Events
{
    public class DonorPledgeEvent
    {
        public Guid DonorId { get; set; }
        public Guid RequestId { get; set; }
        public DateOnly PledgedAt { get; set; }
        public string Status { get; set; } = "Pending";
    }
}