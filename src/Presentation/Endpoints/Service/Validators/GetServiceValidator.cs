using FastEndpoints;
using FluentValidation;

namespace Presentation.Endpoints.Service.Validators
{
    public class GetServiceValidator : Validator<GetServiceRequest>
    {
        public GetServiceValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");
        }
    }
}