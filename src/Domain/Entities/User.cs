namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public ICollection<BloodSac> BloodSacs { get; set; } = new List<BloodSac>();
    }
}