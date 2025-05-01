using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Queries;


namespace Application.Features.BloodBagManagement.Handlers
{
    public class GetBloodBagByIdHandler : IRequestHandler<GetBloodBagByIdQuery, (BloodBagDTO? bloodBag, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<GetBloodBagByIdHandler> _logger;

        public GetBloodBagByIdHandler(IBloodBagRepository bloodBagRepository, ILogger<GetBloodBagByIdHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(BloodBagDTO? bloodBag, BaseException? err)> Handle(GetBloodBagByIdQuery request, CancellationToken ct)
        {
            try
            {
                var bloodBag = await _bloodBagRepository.GetByIdAsync(request.Id);
                if (bloodBag == null)
                {
                    _logger.LogError("Blood bag not found");
                    return (null, new NotFoundException("Blood bag not found", "get blood bag by id"));
                }
                var bloodBagDto = new BloodBagDTO
                {
                    Id = bloodBag.Id,
                    BloodBagType = bloodBag.BloodBagType,
                    BloodType = bloodBag.BloodType,
                    Status = bloodBag.Status,
                    ExpirationDate = bloodBag.ExpirationDate,
                    AcquiredDate = bloodBag.AcquiredDate,
                    DonorId = bloodBag.DonorId ?? Guid.Empty,
                    RequestId = bloodBag.RequestId
                };
                return (bloodBagDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex.Message, "An internal exception occurred");
                return (null, ex);
            }
        }
    }
}
