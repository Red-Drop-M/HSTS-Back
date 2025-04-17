using MediatR;
using Application.Common.Models;
using Domain.Repositories;
using Application.Features.DonorManagement.Commands;

namespace Application.Features.DonorManagement.Handlers
{
    public class UpdateDonorHandler : IRequestHandler<UpdateDonorCommand, Result<bool>>
    {
        private readonly IDonorRepository _donorRepository;

        public UpdateDonorHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<Result<bool>> Handle(UpdateDonorCommand request, CancellationToken cancellationToken)
        {
            
    try
    {
        var donor = await _donorRepository.GetByIdAsync(request.Id);
        if (donor == null)
        {
            return Result<bool>.Failure("Donor not found.");
        }

        // Use the Update method instead of direct property assignment
        donor.Update(
            request.Name,
            request.Email,
            request.DateOfBirth,
            request.BloodType,
            request.NIN,
            request.PhoneNumber,
            request.Address,
            request.LastDonationDate
        );

        await _donorRepository.UpdateAsync(donor);
        return Result<bool>.Success(true);
    }
    catch (Exception ex)
    {
        // Log error
        return Result<bool>.Failure($"Error updating donor: {ex.Message}");
    }
        }
    }
} 