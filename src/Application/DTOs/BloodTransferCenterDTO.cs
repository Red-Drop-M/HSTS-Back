namespace Application.DTOs
{
    public class BloodTransferCenterDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid WilayaId { get; set; }
        public string WilayaName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}