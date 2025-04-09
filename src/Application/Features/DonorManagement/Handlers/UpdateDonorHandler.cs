using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

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
            var donor = await _donorRepository.GetByIdAsync(request.Id);
            if (donor == null)
            {
                return Result<bool>.Failure("Donor not found.");
            }

            donor.Name = request.Name;
            donor.BloodType = request.BloodType;
            // Update other properties as needed

            await _donorRepository.UpdateAsync(donor);
            return Result<bool>.Success(true);
        }
    }
} 