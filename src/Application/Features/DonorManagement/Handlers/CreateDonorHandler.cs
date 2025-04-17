using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.DonorManagement.Commands;

namespace Application.Features.DonorManagement.Handlers
{
    public class CreateDonorHandler : IRequestHandler<CreateDonorCommand, Result<Guid>>
    {
        private readonly IDonorRepository _donorRepository;

        public CreateDonorHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<Result<Guid>> Handle(CreateDonorCommand request, CancellationToken cancellationToken)
        {
            try
            {
        var donor = new Donor(
            name: request.Name,
            email: request.Email,
            dateOfBirth: request.DateOfBirth,
            bloodType: request.BloodType,
            nin: request.NIN,
            phoneNumber: request.PhoneNumber,
            address: request.Address,
            lastDonationDate: request.LastDonationDate
        );

        await _donorRepository.AddAsync(donor);
        return Result<Guid>.Success(donor.Id);
            }
        catch (Exception ex)
            {
        // Log the error
        return Result<Guid>.Failure($"Error creating donor: {ex.Message}");
            }
        }
    }
} 