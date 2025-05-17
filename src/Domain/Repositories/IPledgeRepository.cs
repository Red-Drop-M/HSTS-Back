using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
        // Domain/Interfaces/IRepositories/IDonorRequestPledgeRepository.cs
    public interface IPledgeRepository
    {
        Task<DonorPledge?> GetByIdAsync(string DonorName, Guid RequestId);
        Task<IEnumerable<DonorPledge>> GetByDonorNameAsync(string donorName);
        Task<IEnumerable<DonorPledge>> GetByRequestIdAsync(Guid requestId);
        Task AddAsync(DonorPledge pledge);
        Task UpdateAsync(DonorPledge pledge);
        Task DeleteAsync(string DonorName, Guid RequestId);
    }
}