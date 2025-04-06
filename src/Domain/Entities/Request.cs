using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Request
    {
        // Primary key
        public Guid Id { get; private set; }

        // Value objects
        public Priority Priority { get; private set; } = Priority.Standard();
        public BloodBagType BloodBagType { get; private set; } = BloodBagType.Blood();
        public DateOnly RequestDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DueDate { get; private set; }
        public RequestStatus Status { get; private set; } = RequestStatus.Pending();
        public string? MoreDetails { get; private set; }
        public int RequiredQty { get; private set; } = 1;
        public int AquiredQty { get; private set; } = 0;

        // Foreign keys
        public int? ServiceId { get; private set; }
        public Guid? DonnorId { get; private set; }

        // Navigation properties
        public Service? Service { get; private set; }
        public Donor? Donor { get; private set; }
        public ICollection<BloodBag> BloodSacs { get; private set; } = new List<BloodBag>();

        // Optional constructor for use if you want to create manually
        private Request() { } // for EF Core

        public Request(
            Priority priority,
            BloodBagType bloodBagType,
            DateOnly? dueDate,
            string? moreDetails,
            int requiredQty,
            int? serviceId = null,
            Guid? donnorId = null)
        {
            Priority = priority;
            BloodBagType = bloodBagType;
            DueDate = dueDate;
            MoreDetails = moreDetails;
            RequiredQty = requiredQty;
            ServiceId = serviceId;
            DonnorId = donnorId;
            Status = RequestStatus.Pending();
            RequestDate = DateOnly.FromDateTime(DateTime.Now);
        }

        // Optionally, you can add methods like "MarkAsCompleted", "AddBloodSac", etc.
    }
}
