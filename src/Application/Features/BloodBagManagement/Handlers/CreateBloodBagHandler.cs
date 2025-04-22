using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.Features.BloodBagManagement.Commands;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class CreateBloodBagHandler : IRequestHandler<CreateBloodBagCommand, (BloodBagDTO? bloodBag, BaseException? err)> 
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CreateBloodBagHandler> _logger;

        public CreateBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<CreateBloodBagHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(BloodBagDTO? bloodBag , BaseException? err)> Handle(CreateBloodBagCommand bloodBag, CancellationToken cancellationToken)
        {
            try
            {
                var newBloodBag = new BloodBag(
                    bloodBag.BloodType,
                    bloodBag.BloodBagType,
                    bloodBag.ExpirationDate,
                    bloodBag.AcquiredDate,
                    bloodBag.DonorId,
                    bloodBag.RequestId);

                await _bloodBagRepository.AddAsync(newBloodBag);
                return (new BloodBagDTO
                {
                    Id = newBloodBag.Id,
                    BloodType = newBloodBag.BloodType,
                    BloodBagType = newBloodBag.BloodBagType,
                    ExpirationDate = newBloodBag.ExpirationDate,
                    AcquiredDate = newBloodBag.AcquiredDate,
                    DonorId = newBloodBag.DonorId ?? throw new InvalidOperationException("DonorId cannot be null")
                },null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating blood bag");
                return (null, ex);
            }
        }
    }
}
