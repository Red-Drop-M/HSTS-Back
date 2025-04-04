using Shared.Exceptions;
namespace Domain.ValueObjects
{
    public class UserRole 
    {
        public string Role { get; }
        private UserRole(string role){
            Role = role;
        }
        public static UserRole Admin() => new UserRole("Admin");
        public static UserRole User() => new UserRole("User");
        public static UserRole Convert(string role)
        {
            if(role != "Admin" && role != "User")
            {
                throw new InternalServerException("Invalid role");
            }
            return new UserRole(role);
        }
        public override bool Equals(object? obj)
        {
            if (obj is UserRole other)
            {
                return Role.Equals(other.Role, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        public override int GetHashCode() => Role.ToLowerInvariant().GetHashCode();
        public override string ToString() => Role; 
    }
} 