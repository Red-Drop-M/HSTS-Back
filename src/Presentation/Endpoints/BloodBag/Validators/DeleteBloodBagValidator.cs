using FluentValidation;
using FastEndpoints;

namespace Presentation.Endpoints.BloodBag.Validators
{
    public class DeleteBloodBagValidator : Validator<DeleteBloodBagRequest>
    {
        public DeleteBloodBagValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.")
                .Must(BeAValidGuid).WithMessage("Id must be a valid GUID.");
        }

        private bool BeAValidGuid(Guid id)
        {
            return id != Guid.Empty;
        }
    }
}