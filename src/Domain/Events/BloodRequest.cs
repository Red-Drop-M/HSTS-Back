using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Events
{
    public class BloodRequestEvent
    {
        public Guid Id { get; set; }
        public BloodType BloodType { get; set; }
        public Priority Priority { get; set; }
        public BloodBagType BloodBagType { get; set; }
        public DateOnly RequestDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public RequestStatus Status { get; set; }
        public string? MoreDetails { get; set; }
        public int RequiredQty { get; set; }
        public int AquiredQty { get; set; }
        public string  ServiceName { get; set; }
    }
}