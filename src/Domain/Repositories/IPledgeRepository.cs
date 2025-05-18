using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories
{
    public interface IPledgeRepository
    {
        Task<DonorPledge?> GetByIdAsync(Guid DonorId, Guid RequestId);
        Task<IEnumerable<DonorPledge>> GetByDonorNameAsync(Guid donorId);
        Task<IEnumerable<DonorPledge>> GetByRequestIdAsync(Guid requestId);
        Task<(List<DonorPledge> pledges, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? status = null,
            Guid? donorId = null,
            Guid? requestId = null,
            string? bloodType = null);
        Task AddAsync(DonorPledge pledge);
        Task UpdateAsync(DonorPledge pledge);
        Task DeleteAsync(Guid DonorId, Guid RequestId);
        Task<DonorPledge?> GetByDonorAndRequestIdAsync(Guid donorId, Guid requestId);
    }
}