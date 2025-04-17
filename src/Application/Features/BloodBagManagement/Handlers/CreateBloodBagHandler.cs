using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Application.Features.BloodBagManagement.Commands;
using Domain.Repositories;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class CreateBloodBagHandler : IRequestHandler<CreateBloodBagCommand, Result<Guid>>
    {
        private readonly IBloodBagRepository _bloodBagRepository;

        public CreateBloodBagHandler(IBloodBagRepository bloodBagRepository)
        {
            _bloodBagRepository = bloodBagRepository;
        }

        public async Task<Result<Guid>> Handle(CreateBloodBagCommand request, CancellationToken cancellationToken)
        {
            try {
                var bloodBag = new BloodBag
            (
                bloodType : request.BloodType,
                bloodBagType : request.BloodBagType,
                expirationDonorDate : request.ExpirationDonorDate,
                donorId : request.DonorId
            );

            await _bloodBagRepository.AddAsync(bloodBag);
            return Result<Guid>.Success(bloodBag.Id);
            }
            catch (Exception ex)
            {
                // Log the error
                return Result<Guid>.Failure($"Error creating blood bag: {ex.Message}");
            }

            
            }
            
        }
    }
