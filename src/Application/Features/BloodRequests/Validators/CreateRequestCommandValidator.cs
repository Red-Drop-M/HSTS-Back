using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Application.Features.BloodRequests.Commands;
using Domain.Repositories;

namespace Application.Features.BloodRequests.Validators
{
    public class CreateRequestCommandValidator : AbstractValidator<CreateRequestCommand>
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IDonorRepository _donorRepository;

        public CreateRequestCommandValidator(
            IServiceRepository serviceRepository,
            IDonorRepository donorRepository)
        {
            _serviceRepository = serviceRepository;
            _donorRepository = donorRepository;

            RuleFor(x => x.DonorId)
                .MustAsync(DonorExists)
                .WithMessage("Donor not found.");

            RuleFor(x => x.ServiceId)
                .MustAsync(ServiceExists)
                .WithMessage("Service not found.");
        }

        private async Task<bool> DonorExists(Guid? donorId, CancellationToken cancellationToken)
        {
            if (!donorId.HasValue)
                return true; // Allow null

            var donor = await _donorRepository.GetByIdAsync(donorId.Value)
                                             .ConfigureAwait(false);
            return donor != null;
        }

        private async Task<bool> ServiceExists(Guid? serviceId, CancellationToken cancellationToken)
        {
            if (!serviceId.HasValue)
                return true; // Allow null

            var service = await _serviceRepository.GetByIdAsync(serviceId.Value)
                                                  .ConfigureAwait(false);
            return service != null;
        }
    }
}
