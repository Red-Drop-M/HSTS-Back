using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class DonorPledge
    {
        // Composite primary key (DonorId + RequestId)
        public string DonorName { get; set; } = string.Empty;
        public Guid RequestId { get; private set; }
        
        public PledgeStatus Status { get; private set; }
        public DateOnly PledgeDate { get; private set; }

        // Navigation properties
        public Donor Donor { get; private set; } = null!;
        public Request Request { get; private set; } = null!;

        // Private constructor for EF Core
        private DonorPledge() 
        { 
            Status = PledgeStatus.Pledged; 
            PledgeDate = DateOnly.FromDateTime(DateTime.Now);
        }
           public static DonorPledge FromEvent(DonorPledgeEvent @event) => new DonorPledge(
            @event.DonorName,
            @event.RequestId,
            @event.Status,
            @event.PledgedAt
        );

        public DonorPledge(
            String donorName,
            Guid requestId,
            PledgeStatus status,
            DateOnly pledgeDate)
        {
            DonorName = donorName;
            RequestId = requestId;
            Status = status;
            PledgeDate = pledgeDate;
        }

        public void UpdateStatus(PledgeStatus newStatus)
        {
            Status = newStatus;
        }
    }
}