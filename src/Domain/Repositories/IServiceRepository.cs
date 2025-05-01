using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(Guid id);
        Task<Service?> GetByNameAsync(string name);

        Task AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(Guid id);
        Task<List<Service?>> GetServicesAsync();

        Task<(List<Service?> Services, int Total)> GetAllAsync(int page, int pageSize, ServiceFilter filter);
    }
}