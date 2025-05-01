using FastEndpoints;
using FluentValidation;
using Domain.ValueObjects;

namespace Presentation.Endpoints.Service.Validators
{
    public class UpdateServiceValidator : Validator<UpdateServiceRequest>
    {
        public UpdateServiceValidator()
        {
           /* RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");*/
                // Commented

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must not exceed 100 characters.");
        }
    }
}