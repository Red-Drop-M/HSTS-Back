using Domain.Entities;

namespace Domain.Repositories
{
    public interface IWilayaRepository
    {
        Task<Wilaya?> GetByIdAsync(Guid id);
        Task<Wilaya?> GetByNameAsync(string name);
        Task<List<Wilaya>> GetAllAsync();
        Task<(List<Wilaya> Wilayas, int Total)> GetAllAsync(int page, int pageSize, string? name = null);
        Task<Wilaya> AddAsync(Wilaya wilaya);
        Task UpdateAsync(Wilaya wilaya);
        Task DeleteAsync(Guid id);
    }
}