using MediatR;
using Application.Common.Models;
using Domain.Repositories;
using Application.Features.BloodBagManagement.Commands;

namespace Application.Features.BloodBagManagement.Handlers
{
    public class DeleteBloodBagHandler : IRequestHandler<DeleteBloodBagCommand, Result<bool>>
    {
        private readonly IBloodBagRepository _bloodBagRepository;

        public DeleteBloodBagHandler(IBloodBagRepository bloodBagRepository)
        {
            _bloodBagRepository = bloodBagRepository;
        }

        public async Task<Result<bool>> Handle(DeleteBloodBagCommand request, CancellationToken cancellationToken)
        {
            var bloodBag = await _bloodBagRepository.GetByIdAsync(request.Id);
            if (bloodBag == null)
            {
                return Result<bool>.Failure("Blood bag not found.");
            }

            await _bloodBagRepository.DeleteAsync(bloodBag.Id);
            return Result<bool>.Success(true);
        }
    }
} 