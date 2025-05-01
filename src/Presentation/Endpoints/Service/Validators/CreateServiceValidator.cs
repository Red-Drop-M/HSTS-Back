using FluentValidation;
using Presentation.Endpoints.Service;

namespace HSTS_Back.Presentation.Endpoints.Service.Validators
{
    public class CreateServiceValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceValidator()
        {
            RuleFor(service => service.Name)
                .NotEmpty().WithMessage("Service name is required.")
                .MaximumLength(100).WithMessage("Service name must not exceed 100 characters.");
        }
    }
}