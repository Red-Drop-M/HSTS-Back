using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
        // Domain/Interfaces/IRepositories/IDonorRequestPledgeRepository.cs
    public interface IDonorPledgeRepository
    {
        Task<DonorPledge> GetByIdAsync(Guid DonorId, Guid RequestId);
       Task<IEnumerable<DonorPledge>> GetByDonorIdAsync(Guid donorId);
    Task<IEnumerable<DonorPledge>> GetByRequestIdAsync(Guid requestId);
        Task AddAsync(DonorPledge pledge);
        Task UpdateAsync(DonorPledge pledge);
        Task DeleteAsync(Guid DonorId, Guid RequestId);
    }
}