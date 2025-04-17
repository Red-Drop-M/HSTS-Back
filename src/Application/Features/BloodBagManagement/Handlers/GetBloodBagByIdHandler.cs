using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Queries;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class GetBloodBagByIdHandler : IRequestHandler<GetBloodBagByIdQuery, Result<BloodBag?>>
    {
        private readonly IBloodBagRepository _bloodBagRepository;

        public GetBloodBagByIdHandler(IBloodBagRepository bloodBagRepository)
        {
            _bloodBagRepository = bloodBagRepository;
        }

        public async Task<Result<BloodBag?>> Handle(GetBloodBagByIdQuery request, CancellationToken cancellationToken)
        {
            var bloodBag = await _bloodBagRepository.GetByIdAsync(request.Id);
            return Result<BloodBag?>.Success(bloodBag);
        }
    }
} 