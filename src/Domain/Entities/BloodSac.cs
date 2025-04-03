namespace Domain.Entities{
    public class BloodSac
    {
        public int Id { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public int Volume { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation propertie
    }
}