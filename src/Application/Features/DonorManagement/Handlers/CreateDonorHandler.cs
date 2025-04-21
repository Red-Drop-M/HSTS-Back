using MediatR;
using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.DonorManagement.Commands;

namespace Application.Features.DonorManagement.Handlers
{
    public class CreateDonorHandler : IRequestHandler<CreateDonorCommand, DonorDTO>
    {
        private readonly IDonorRepository _donorRepository;
        private readonly ILogger<CreateDonorHandler> _logger;

        CreateDonorHandler(IDonorRepository donorRepository, ILogger<CreateDonorHandler> logger)
        {
            _donorRepository = donorRepository;
            _logger = logger;
        }

        public async Task<DonorDTO> Handle(CreateDonorCommand Donor , CancellationToken cancellationToken)
        {
            try
            {
                var newDonor = new Donor(
                    Donor.Name,
                    Donor.Email,
                    Donor.LastDonationDate,
                    Donor.BloodType,
                    Donor.Address,
                    Donor.NIN,
                    Donor.PhoneNumber,
                    Donor.DateOfBirth);

                await _donorRepository.AddAsync(newDonor);
                return new DonorDTO
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
                };
            }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating donor");
                    throw;
                }

        }
    }
        
} 