using FastEndpoints;
using FluentValidation;

namespace Presentation.Endpoints.BloodBag.Validators
{
    public class GetBloodBagValidator : Validator<GetBloodBagRequest>
    {
        public GetBloodBagValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");
        }
    }
}