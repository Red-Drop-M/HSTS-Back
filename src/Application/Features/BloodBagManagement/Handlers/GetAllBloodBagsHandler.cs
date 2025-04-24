using Application.DTOs;
using Domain.ValueObjects;
using MediatR;
using Domain.Repositories;
using Shared.Exceptions;
using Application.Features.BloodBagManagement.Queries;

namespace Application.Features.BloodBagManagement.Handlers
{

public class GetAllBloodBagsHandler : IRequestHandler<GetAllBloodBagsQuery, (List<BloodBagDTO>? BloodBags, int total, BaseException? err)>
{
    private readonly IBloodBagRepository _bloodBagRepository;
    private readonly ILogger<GetAllBloodBagsHandler> _logger;

    public GetAllBloodBagsHandler(IBloodBagRepository bloodBagRepository, ILogger<GetAllBloodBagsHandler> logger)
    {
        _bloodBagRepository = bloodBagRepository;
        _logger = logger;
    }

    public async Task<(List<BloodBagDTO>? BloodBags, int total, BaseException? err)> Handle(GetAllBloodBagsQuery BloodBag, CancellationToken cancellationToken)
    {
        try
        {
            var filter = new BloodBagFilter
            {
                BloodBagType = BloodBag.BloodBagType,
                BloodType = BloodBag.BloodType,
                Status = BloodBag.Status,
                ExpirationDate = BloodBag.ExpirationDate,
                AcquiredDate = BloodBag.AcquiredDate,
                DonorId = BloodBag.DonorId,
                RequestId = BloodBag.RequestId
            };

            var (bloodBags, total) = await _bloodBagRepository.GetAllAsync(BloodBag.PageNumber, BloodBag.PageSize, filter);
            if (bloodBags == null || !bloodBags.Any())
            {
                _logger.LogWarning("No blood bags found");
                return (null, 0, new NotFoundException("No blood bags found", "Fetching blood bags"));
            }

            var bloodBagDtos = bloodBags.Select(b => new BloodBagDTO
            {
                Id = b.Id,
                BloodType = b.BloodType,
                BloodBagType = b.BloodBagType,
                ExpirationDate = b.ExpirationDate,
                AcquiredDate = b.AcquiredDate,
                DonorId = b.DonorId ?? Guid.Empty,
                RequestId = b.RequestId
            }).ToList();

            return (bloodBagDtos, total, null);
        }
        catch (BaseException ex)
        {
            _logger.LogError(ex, "Error fetching blood bags");
            return (null, 0, ex);
        }
    }
}

}

