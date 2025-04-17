using MediatR;
using Application.Common.Models;
using Application.Features.DonorManagement.Commands;
using Domain.Repositories;

namespace Application.Features.DonorManagement.Handlers
{
    public class DeleteDonorHandler : IRequestHandler<DeleteDonorCommand, Result<bool>>
    {
        private readonly IDonorRepository _donorRepository;

        public DeleteDonorHandler(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public async Task<Result<bool>> Handle(DeleteDonorCommand request, CancellationToken cancellationToken)
        {
            var donor = await _donorRepository.GetByIdAsync(request.Id);
            if (donor == null)
            {
                return Result<bool>.Failure("Donor not found.");
            }

            await _donorRepository.DeleteAsync(donor.Id);
            return Result<bool>.Success(true);
        }
    }
} 