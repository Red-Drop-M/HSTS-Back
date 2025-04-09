using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

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
            var bloodBag = new BloodBag
            {
                BloodType = request.BloodType,
                Quantity = request.Quantity,
                // Initialize other properties as needed
            };

            await _bloodBagRepository.AddAsync(bloodBag);
            return Result<Guid>.Success(bloodBag.Id);
        }
    }
} 