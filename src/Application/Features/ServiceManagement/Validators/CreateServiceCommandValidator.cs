using FluentValidation;
using Application.Features.ServiceManagement.Commands;

namespace Application.Features.ServiceManagement.Validators
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Service name is required.")
                .MaximumLength(100).WithMessage("Service name must not exceed 100 characters.");

        }
    }
}