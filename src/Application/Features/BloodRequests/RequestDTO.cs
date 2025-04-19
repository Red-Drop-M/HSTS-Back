using Domain.Entities;
using Domain.ValueObjects;



namespace Application.Features.BloodRequests
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public Priority Priority { get; set; } = Priority.Standard();
        public BloodType BloodGroup { get; set; } = BloodType.APositive();
        public BloodBagType BloodBagType { get; set; } = BloodBagType.Blood();
        public DateOnly RequestDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending();
        public string? MoreDetails { get; set; }
        public int RequiredQty { get; set; }
        public int AquiredQty { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? DonorId { get; set; }
    }
}



