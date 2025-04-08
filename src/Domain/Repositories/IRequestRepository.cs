using Domain.Entities;
using Domain.ValueObjects;
namespace Domain.Repositories
{
    public interface IRequestRepository
    {
       
        Task<Request?> GetByIdAsync(Guid id);
        Task<Request?> GetByServiceIdAsync(Guid serviceId);
        Task<Request?> GetByStatusAsync(RequestStatus status);
        Task AddAsync(Request request);
        Task UpdateAsync(Request request);
        Task DeleteAsync(Guid id);
        Task <List<Request>> GetByBloodBagTypeAsync(BloodBagType bloodBagType);
        Task<List<Request>> GetByPriorityAsync(Priority priority);
        Task<List<Request>> GetByRequestDateAsync(DateOnly requestDate);
        Task<List<Request>> GetByDueDateAsync(DateOnly dueDate);
        Task<List<Request>> GetAllAsync();
        Task<List<Request>> GetByDonorIdAsync(Guid donorId);
    }
}