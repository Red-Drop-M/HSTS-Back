namespace Domain.ValueObjects
{
    public class RequestFilter
    {
        public string? Priority { get; set; }
        public string? BloodBagType { get; set; }
        public string? RequestDate { get; set; }
        public string? DueDate { get; set; }
        public string? DonorId { get; set; }
        public string? ServiceId { get; set; }
        public string? Status { get; set; }
        public string? BloodType { get; set; }
    }
}
