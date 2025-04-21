using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Application.Features.BloodBagManagement.Commands;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class CreateBloodBagHandler : IRequestHandler<CreateBloodBagCommand, BloodBagDTO> 
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<CreateBloodBagHandler> _logger;

        public CreateBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<CreateBloodBagHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<BloodBagDTO> Handle(CreateBloodBagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newBloodBag = new BloodBag(
                    request.BloodType,
                    request.BloodBagType,
                    request.ExpirationDate,
                    request.DonorId);

                await _bloodBagRepository.AddAsync(newBloodBag);
                return new BloodBagDTO
                {
                    Id = newBloodBag.Id,
                    BloodType = newBloodBag.BloodType,
                    BloodBagType = newBloodBag.BloodBagType,
                    ExpirationDate = newBloodBag.ExpirationDate,
                    DonorId = newBloodBag.DonorId ?? throw new InvalidOperationException("DonorId cannot be null")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blood bag");
                throw;
            }
        }
    }
}
