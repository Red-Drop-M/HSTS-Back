using Domain.ValueObjects;

namespace Application.DTOs
{
    public class DonorPledgeListDTO
    {
        public Guid DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public Guid RequestId { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public DateOnly PledgeDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}