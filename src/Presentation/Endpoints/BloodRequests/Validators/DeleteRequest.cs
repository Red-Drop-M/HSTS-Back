using FluentValidation;
using FastEndpoints;
using Presentation.Endpoints.BloodRequests;

namespace Presentation.Endpoints.BloodRequests.Validators
{
    public class DeleteRequestValidator : Validator<DeleteRequestRequest>
    {
        public DeleteRequestValidator()
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