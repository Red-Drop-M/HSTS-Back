namespace Domain.ValueObjects
{
    public class RequestFilter
    {
        public Guid? ServiceId { get; set; }
        public Guid? DonorId { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? BloodType { get; set; }
        public string? BloodBagType { get; set; }
        public string? RequestDate { get; set; }
        public string? DueDate { get; set; }
        public Guid? RequestId { get; set; }  // Add this property
    }
}
