using MediatR;
using Domain.ValueObjects;
using Shared.Exceptions;
using Application.DTOs;
using Domain.Repositories;
using Application.Features.DonorManagement.Commands;
using System.Linq.Expressions;

namespace Application.Features.DonorManagement.Handlers
{
    
    public class UpdateDonorHandler : IRequestHandler<UpdateDonorCommand, (DonorDTO? donor, BaseException? err)>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<UpdateDonorHandler> _logger;

        public UpdateDonorHandler(IDonorRepository donorRepository, ILogger<UpdateDonorHandler> logger)
        {
            _donorRepository = donorRepository;
            _logger = logger;
        }

        public async Task<(DonorDTO? donor, BaseException? err)> Handle(UpdateDonorCommand Donor, CancellationToken cancellationToken)
        {
            try {

                var donor = await _donorRepository.GetByIdAsync(Donor.Id);
                if (donor == null)
                {
                    _logger.LogError("Donor with ID {DonorId} not found", Donor.Id);
                    return (null, new NotFoundException($"Donor {Donor.Id} not found", "update donor"));
                }

                donor.UpdateDetails(
                    Donor.Name,
                    Donor.BirthDate,
                    Donor.Email,
                    Donor.PhoneNumber,
                    Donor.Address,
                    Donor.NIN,
                    Donor.BloodType,
                    Donor.LastDonationDate
                );
                await _donorRepository.UpdateAsync(donor);
                _logger.LogInformation("Donor with ID {DonorId} updated successfully", Donor.Id);

                var donorDto = new DonorDTO
                {
                    Id = donor.Id,
                    Name = donor.Name,
                    Email = donor.Email,
                    LastDonationDate = donor.LastDonationDate,
                    BloodType = donor.BloodType,
                    Address = donor.Address,
                    NIN = donor.NIN,
                    PhoneNumber = donor.PhoneNumber,
                    DateOfBirth = donor.DateOfBirth
                };
                return (donorDto, null);
            }

            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error while updating donor with ID {DonorId}", Donor.Id);
                return (null, ex);
            }
        }
    }

} 