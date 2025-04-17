using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Queries;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class GetAllBloodBagsHandler : IRequestHandler<GetAllBloodBagsQuery, Result<List<BloodBag?>>>
    {
        private readonly IBloodBagRepository _bloodBagRepository;

        public GetAllBloodBagsHandler(IBloodBagRepository bloodBagRepository)
        {
            _bloodBagRepository = bloodBagRepository;
        }

        public async Task<Result<List<BloodBag?>>> Handle(GetAllBloodBagsQuery request, CancellationToken cancellationToken)
        {
            var bloodBags = await _bloodBagRepository.GetAllAsync();
            return Result<List<BloodBag?>>.Success(bloodBags);
        }
    }
} 