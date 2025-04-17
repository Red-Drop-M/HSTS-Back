using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IDonorRepository
    {
        Task<Donor?> GetByIdAsync(Guid id);
        Task<List<Donor>> GetAllAsync();
        Task<Donor?> GetByEmailAsync(string email);
        Task<Donor?> GetByPhoneNumberAsync(string phoneNumber);
        Task<Donor?> GetByAddressAsync(string address);
        Task<Donor?> GetByNINAsync(string nin);
        Task<Donor?> GetByNameAsync(string name);
        Task<List<Donor>> GetByBloodGroupAsync(BloodType bloodGroup);
        Task<List<Donor>> GetByLastDonnationDateAsync(DateOnly date);
        Task AddAsync(Donor donor);
        Task UpdateAsync(Donor donor);
        Task DeleteAsync(Guid id);
    }
}