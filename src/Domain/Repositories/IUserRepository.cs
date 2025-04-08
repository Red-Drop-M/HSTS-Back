using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetByAddressAsync(string address);
        Task<User?> GetByNINAsync(string nin);
        Task<User?> GetByNameAsync(string name);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}