using Domain.ValueObjects;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get;private set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User(); 
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        protected User() { } // EF Core requires a parameterless constructor
        public User(string name, string email, string password, UserRole role, DateTime dateOfBirth, string phoneNumber, string address)
        {
            Name = name;
            Email = email;
            Password = password;
            Role = role;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
            Address = address;
        }
    }
}