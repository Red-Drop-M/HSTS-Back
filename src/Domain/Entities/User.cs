using Domain.ValueObjects;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.User(); // Default value
        public DateTime DateOfBirth { get; private set; }
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;

        // EF Core requires a parameterless constructor
        private User() { }

        public User(
            string name,
            string email,
            string password,
            UserRole role,
            DateTime dateOfBirth,
            string phoneNumber,
            string address)
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