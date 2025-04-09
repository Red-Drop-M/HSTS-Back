using MediatR;
using Application.Common.Models;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Application.Features.DonorManagement.Handlers
{
    public class GetAllDonorsHandler : IRequestHandler<GetAllDonorsQuery, Result<List<Donor?>>>
    {
        private readonly IDonorRepository _donorRepository;

        public GetAllDonorsHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<Result<List<Donor?>>> Handle(GetAllDonorsQuery request, CancellationToken cancellationToken)
        {
            var donors = await _donorRepository.GetAllAsync();
            return Result<List<Donor?>>.Success(donors);
        }
    }
} 