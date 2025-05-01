using Domain.Entities;
using Domain.Repositories;
using MediatR;
using Application.DTOs;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using Application.Features.BloodBagManagement.Commands;
using Domain.ValueObjects;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class UpdateBloodBagHandler : IRequest<(BloodBagDTO? bloodBag, BaseException? err)>
    {
        private readonly IBloodBagRepository _bloodBagRepository;
        private readonly ILogger<UpdateBloodBagHandler> _logger;

        public UpdateBloodBagHandler(IBloodBagRepository bloodBagRepository, ILogger<UpdateBloodBagHandler> logger)
        {
            _bloodBagRepository = bloodBagRepository;
            _logger = logger;
        }

        public async Task<(BloodBagDTO? bloodBag, BaseException? err)> Handle(UpdateBloodBagCommand command, CancellationToken ct)
        {
            try
            {
                var bloodBag = await _bloodBagRepository.GetByIdAsync(command.Id);
                if (bloodBag == null)
                {
                    _logger.LogError("Blood bag not found");
                    throw new NotFoundException("No blood bag found with the provided ID", "Updating blood bag");
                }

                bloodBag.UpdateDetails(command.BloodBagType,command.BloodType,  command.ExpirationDate,command.AcquiredDate,command.RequestId);
                await _bloodBagRepository.UpdateAsync(bloodBag);

                _logger.LogInformation("Blood bag updated successfully");

                var bloodBagDto = new BloodBagDTO
                {
                    Id = bloodBag.Id,
                    BloodType = bloodBag.BloodType,
                    BloodBagType = bloodBag.BloodBagType,
                    ExpirationDate = bloodBag.ExpirationDate,
                    AcquiredDate = bloodBag.AcquiredDate,
                    Status = bloodBag.Status,
                    DonorId = bloodBag.DonorId ?? Guid.Empty,
                    RequestId = bloodBag.RequestId
                };

                return (bloodBagDto, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError("Error while updating blood bag: {Message}", ex.Message);
                return (null, ex);
            }
        }
    }
}