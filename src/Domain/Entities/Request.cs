using Domain.ValueObjects;
using Newtonsoft.Json;

namespace Domain.Entities
{
    public class Request
    {
        public Guid Id { get; private set; }
        public BloodType BloodType { get; private set; } = BloodType.APositive();
        public Priority Priority { get; private set; } = Priority.Standard();
        public BloodBagType BloodBagType { get; private set; } = BloodBagType.Blood();
        public DateOnly RequestDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DueDate { get; private set; }
        public RequestStatus Status { get; private set; } = RequestStatus.Pending();
        public string? MoreDetails { get; private set; }
        public int RequiredQty { get; private set; } = 1;
        public int AquiredQty { get; private set; } = 0;

        // Foreign keys
        public Guid? ServiceId { get; private set;}
        public Guid? DonorId { get; private set; }

        // Navigation properties
        public Service? Service { get; private set; }
        public Donor? Donor { get; private set; }
        public ICollection<BloodBag> BloodSacs { get; private set; } = new List<BloodBag>();
        public ICollection<DonorPledge> Pledges { get; private set; } = new List<DonorPledge>();

        private Request() { }

        // Existing constructor
        public Request(
            BloodType bloodType,
            Priority priority,
            BloodBagType bloodBagType,
            DateOnly? dueDate,
            string? moreDetails,
            int requiredQty,
            Guid? serviceId = null,
            Guid? donorId = null)
        {
            Id = Guid.NewGuid();
            BloodType = bloodType;
            Priority = priority;
            BloodBagType = bloodBagType;
            DueDate = dueDate;
            MoreDetails = moreDetails;
            RequiredQty = requiredQty;
            ServiceId = serviceId;
            DonorId = donorId;
            Status = RequestStatus.Pending();
            RequestDate = DateOnly.FromDateTime(DateTime.Now);
        }

        // New constructor with all attributes except Id
        public Request(
            BloodType bloodType,
            Priority priority,
            BloodBagType bloodBagType,
            DateOnly requestDate,
            DateOnly? dueDate,
            RequestStatus status,
            string? moreDetails,
            int requiredQty,
            int aquiredQty,
            Guid? serviceId,
            Guid? donorId)
        {
            Id = Guid.NewGuid(); // Add this line to generate a new GUID
            BloodType = bloodType;
            Priority = priority;
            BloodBagType = bloodBagType;
            RequestDate = requestDate;
            DueDate = dueDate;
            Status = status;
            MoreDetails = moreDetails;
            RequiredQty = requiredQty;
            AquiredQty = aquiredQty;
            ServiceId = serviceId;
            DonorId = donorId;
        }
        public void UpdateAquiredQty()
        {
            AquiredQty++;
            RequiredQty--;
            if (RequiredQty == 0)
            {
                Status = RequestStatus.Resolved();
            }
        }
        public void Resolve()
        {
            Status = RequestStatus.Resolved();
        }
        public void Reject()
        {
            Status = RequestStatus.Rejected();
        }
        public void UpdateDetails(BloodBagType? bloodBagType, Priority? priority, DateOnly? dueDate, string? moreDetails, int? requiredQty)
        {
            if (bloodBagType is not null) BloodBagType = bloodBagType;
            if (priority is not null) Priority = priority;
            if (dueDate is not null) DueDate = dueDate;
            if (moreDetails is not null) MoreDetails = moreDetails;
            if (requiredQty is not null) RequiredQty = requiredQty.Value;
        }
    }
}
