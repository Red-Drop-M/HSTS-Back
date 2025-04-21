using MediatR;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Commands;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class DeleteBloodBagHandler : IRequestHandler<DeleteBloodBagCommand,  (BloodBagDTO? BloodBag, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<DeleteBloodBagHandler> _logger;

        public DeleteBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<DeleteBloodBagHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(BloodBagDTO? BloodBag, BaseException? err)> Handle(DeleteBloodBagCommand BloodBag, CancellationToken cancellationToken)
        {
            try 
            {
                var bloodBag = await _bloodBagRepository.GetByIdAsync(BloodBag.Id);
                if (bloodBag == null)
                {
                    _logger.LogError("Blood bag with ID {BloodBagId} not found", BloodBag.Id);
                    return (null, new NotFoundException($"Blood bag {BloodBag.Id} not found", "delete blood bag"));
                }

                await _bloodBagRepository.DeleteAsync(bloodBag.Id);

                var bloodBagDto = new BloodBagDTO
                {
                    Id = bloodBag.Id,
                    BloodType = bloodBag.BloodType,
                    BloodBagType = bloodBag.BloodBagType,
                    ExpirationDate = bloodBag.ExpirationDate,
                    DonorId = bloodBag.DonorId ?? throw new InvalidOperationException("DonorId cannot be null")
                };

                return (bloodBagDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Failed to delete blood bag {BloodBagId}", BloodBag.Id);
                return (null, ex);
            }
        }
    }
} 