using FluentValidation;
using FastEndpoints;

namespace Presentation.Endpoints.Service.Validators
{
    public class DeleteServiceValidator : Validator<DeleteServiceRequest>
    {
        public DeleteServiceValidator()
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