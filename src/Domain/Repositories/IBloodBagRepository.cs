using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IBloodBagRepository
    {   
        
        Task<BloodBag?> GetByIdAsync(Guid id);
        Task<List<BloodBag?>> GetByBloodGroupAsync(BloodType bloodGroup);
        Task<List<BloodBag?>> GetByBloodBagTypeAsync(BloodBagType bloodBagType);
        Task<List<BloodBag?>> GetByExpiryDateAsync(DateOnly expiryDate);

        Task<List<BloodBag?>> GetByAcquiredDateAsync(DateOnly acquiredDate);
        Task<List<BloodBag?>> GetByStatusAsync(BloodBagStatus status);
        Task <List<BloodBag?>> GetByDonorIdAsync(Guid DonorId);
        Task<List<BloodBag>> GetByRequestIdAsync(Guid requestId);
        Task<List<BloodBag>> GetByRequestIdsAsync(List<Guid> requestIds);
        Task AddAsync(BloodBag bloodBag);
        Task UpdateAsync(BloodBag bloodBag);
        Task DeleteAsync(Guid id);

        Task<(List<BloodBag> BloodBags, int Total)> GetAllAsync(int page, int pageSize, BloodBagFilter filter);

    }
}