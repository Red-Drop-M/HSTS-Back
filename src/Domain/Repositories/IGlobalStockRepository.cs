using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IGlobalStockRepository
    {
        Task<GlobalStock?> GetByIdAsync(Guid id);
        Task<GlobalStock?> GetByBloodBagTypeAsync(BloodBagType bloodBagType);
        Task<GlobalStock?> GetByBloodGroupAsync(BloodType bloodGroup);
        Task AddAsync(GlobalStock globalStock);
        Task UpdateAsync(GlobalStock globalStock);
        Task DeleteAsync(Guid id);
    }
}