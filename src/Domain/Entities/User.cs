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
            Id = Guid.NewGuid(); // Generate new ID
            Name = name;
            Email = email;
            Password = password;
            Role = role;
            DateOfBirth = dateOfBirth;
            PhoneNumber = phoneNumber;
            Address = address;
        }

        // Update user details
        public void UpdateDetails(
            string? name = null,
            string? email = null,
            string? phoneNumber = null,
            string? address = null,
            DateTime? dateOfBirth = null)
        {
            if (name != null) Name = name;
            if (email != null) Email = email;
            if (phoneNumber != null) PhoneNumber = phoneNumber;
            if (address != null) Address = address;
            if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth.Value;
        }

        // Update user role (admin only operation)
        public void UpdateRole(UserRole newRole)
        {
            Role = newRole;
        }

        // Change password
        public void ChangePassword(string newPassword)
        {
            Password = newPassword;
        }

        // Verify password (for authentication)
        public bool VerifyPassword(string providedPassword)
        {
            return Password == providedPassword;
        }
    }
}