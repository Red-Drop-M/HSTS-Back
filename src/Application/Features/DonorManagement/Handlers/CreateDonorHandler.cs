using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

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
            var donor = new Donor
            {
                Name = request.Name,
                BloodType = request.BloodType,
                // Initialize other properties as needed
            };

            await _donorRepository.AddAsync(donor);
            return Result<Guid>.Success(donor.Id);
        }
    }
} 