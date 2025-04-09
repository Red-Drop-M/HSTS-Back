using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

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
            var bloodBag = await _bloodBagRepository.GetByIdAsync(request.Id);
            if (bloodBag == null)
            {
                return Result<bool>.Failure("Blood bag not found.");
            }

            bloodBag.BloodType = request.BloodType;
            bloodBag.Quantity = request.Quantity;
            // Update other properties as needed

            await _bloodBagRepository.UpdateAsync(bloodBag);
            return Result<bool>.Success(true);
        }
    }
} 