using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Application.Features.DonorManagement.Handlers
{
    public class GetDonorByIdHandler : IRequestHandler<GetDonorByIdQuery, Result<Donor?>>
    {
        private readonly IDonorRepository _donorRepository;

        public GetDonorByIdHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<Result<Donor?>> Handle(GetDonorByIdQuery request, CancellationToken cancellationToken)
        {
            var donor = await _donorRepository.GetByIdAsync(request.Id);
            return Result<Donor?>.Success(donor);
        }
    }
} 