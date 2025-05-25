using Domain.Entities;

namespace Domain.Repositories
{
    public interface ICommuneRepository
    {
        Task<Commune?> GetByIdAsync(Guid id);
        Task<Commune?> GetByNameAsync(string name);
        Task<List<Commune>> GetByWilayaIdAsync(Guid wilayaId);
        Task<List<Commune>> GetAllAsync();
        Task<(List<Commune> Communes, int Total)> GetAllAsync(int page, int pageSize, string? name = null, Guid? wilayaId = null);
        Task AddAsync(Commune commune);
        Task UpdateAsync(Commune commune);
        Task DeleteAsync(Guid id);
    }
}