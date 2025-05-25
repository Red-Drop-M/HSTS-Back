using Domain.Entities;

namespace Domain.Repositories
{
    public interface IBloodTransferCenterRepository
    {
        Task<BloodTransferCenter?> GetByIdAsync(Guid id);
        Task<BloodTransferCenter?> GetByNameAsync(string name);
        Task<BloodTransferCenter?> GetByEmailAsync(string email);
        Task<BloodTransferCenter?> GetPrimaryAsync(); // Add this method
        Task<List<BloodTransferCenter>> GetByWilayaIdAsync(Guid wilayaId);
        Task<List<BloodTransferCenter>> GetAllAsync();
        Task<(List<BloodTransferCenter> Centers, int Total)> GetAllAsync(int page, int pageSize);
        Task AddAsync(BloodTransferCenter center);
        Task UpdateAsync(BloodTransferCenter center);
        Task DeleteAsync(Guid id);
        Task SetAsPrimaryAsync(Guid id); // Add this method
    }
}