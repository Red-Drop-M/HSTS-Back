using Domain.Entities;
using Domain.ValueObjects;
using Application.DTOs;
namespace Application.DTOs
{
    public class RequestDto
    {
        public Guid Id { get; set; }
        public String Priority { get; set; } = String.Empty;
        public String BloodType { get; set; } = String.Empty;
        public String BloodBagType { get; set; } = String.Empty;
        public DateOnly RequestDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public String Status { get; set; } = String.Empty;
        public string? MoreDetails { get; set; }
        public int RequiredQty { get; set; }
        public int AquiredQty { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? DonorId { get; set; }
        
        // New properties for related entities
        public string Service { get; set; }
        public List<BloodBagDTO> BloodBags { get; set; } = new List<BloodBagDTO>();
    }
}



