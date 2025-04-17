using MediatR;
using Application.Common.Models;
using Application.Features.BloodBagManagement.Commands;
using Domain.Repositories;
using Domain.ValueObjects;


namespace Application.Features.BloodBagManagement.Handlers
{
    public class UpdateBloodBagHandler : IRequestHandler<UpdateBloodBagCommand, Result<bool>>
    {
        private readonly IBloodBagRepository _bloodBagRepository;

        public UpdateBloodBagHandler(IBloodBagRepository bloodBagRepository)
        {
            _bloodBagRepository = bloodBagRepository;
        }

        public async Task<Result<bool>> Handle(UpdateBloodBagCommand request, CancellationToken cancellationToken)
        {
            
    try
    {
        var bloodBag = await _bloodBagRepository.GetByIdAsync(request.Id);
        if (bloodBag == null)
        {
            return Result<bool>.Failure("Blood bag not found.");
        }


        bloodBag.Update(
            bloodType: request.BloodType,
            bloodBagType: request.BloodBagType,
            expirationDate: request.ExpirationDonorDate,
            donorId: request.DonorId,
            requestId: request.RequestId
        );

        await _bloodBagRepository.UpdateAsync(bloodBag);
        return Result<bool>.Success(true);
    }
    catch (Exception ex)
    {
        return Result<bool>.Failure($"Error updating blood bag: {ex.Message}");
    }
        }
    }
}

