using FluentValidation;
using Application.Features.ServiceManagement.Commands;
using Domain.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.ServiceManagement.Validators
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        private readonly IServiceRepository _serviceRepository;

        public CreateServiceCommandValidator(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Service name is required.")
                .MaximumLength(100).WithMessage("Service name must not exceed 100 characters.")
                .MustAsync(BeUniqueName).WithMessage("Service name must be unique.");
        }

        private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetByNameAsync(name).ConfigureAwait(false);
            return service == null;
        }
    }
}