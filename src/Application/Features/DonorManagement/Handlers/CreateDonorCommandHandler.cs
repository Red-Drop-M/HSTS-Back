using MediatR;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.DonorManagement.Commands;
using Shared.Exceptions;

namespace Application.Features.DonorManagement.Handlers
{
   public class CreateDonorCommandHandler : IRequestHandler<CreateDonorCommand, (DonorDTO? donor, Exception? err)>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<CreateDonorCommandHandler> _logger;

        public CreateDonorCommandHandler(IDonorRepository donorRepository, ILogger<CreateDonorCommandHandler> logger)
        {
            _donorRepository = donorRepository;
            _logger = logger;
        }

        public async Task<(DonorDTO? donor, Exception? err)> Handle(CreateDonorCommand Donor, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating donor with name: {Name}", Donor.Name);
                var newDonor = new Donor(
                    Donor.Name,
                    Donor.Email,
                    Donor.BloodType,
                    Donor.LastDonationDate,
                    Donor.Address,
                    Donor.NIN,
                    Donor.PhoneNumber,
                    Donor.DateOfBirth);

                await _donorRepository.AddAsync(newDonor);
                return (new DonorDTO
                {
                    Id = newDonor.Id,
                    Name = newDonor.Name,
                    Email = newDonor.Email,
                    LastDonationDate = newDonor.LastDonationDate,
                    BloodType = newDonor.BloodType,
                    Address = newDonor.Address,
                    NIN = newDonor.NIN,
                    PhoneNumber = newDonor.PhoneNumber,
                    DateOfBirth = newDonor.DateOfBirth
                },null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating donor");
                return (null,ex);
            }

        }
    }
        
} 