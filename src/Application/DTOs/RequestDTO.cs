using Domain.Entities;
using Domain.ValueObjects;



namespace Application.DTOs
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public String Priority { get; set; } = String.Empty;
        public String BloodType { get; set; } = String.Empty;
        public String BloodBagType { get; set; }= String.Empty;
        public DateOnly RequestDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public String Status { get; set; } = String.Empty;
        public string? MoreDetails { get; set; }
        public int RequiredQty { get; set; }
        public int AquiredQty { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? DonorId { get; set; }
    }
}



