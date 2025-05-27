using FastEndpoints;
using FluentValidation;
using Domain.ValueObjects;

namespace Presentation.Endpoints.BloodRequests.Validators
{
    public class UpdateRequestValidator : Validator<UpdateRequestRequest>
    {
        public UpdateRequestValidator()
        {
            // Fix: change 'Id' to 'id' to match property name in UpdateRequestRequest
            RuleFor(x => x.id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");
                
            // Add null checks for optional properties
            When(x => x.BloodBagType != null, () => {
                RuleFor(x => x.BloodBagType)
                    .Must(x => BloodBagType.Convert(x) != null)
                    .WithMessage("Invalid BloodBagType.");
            });

            When(x => x.Priority != null, () => {
                RuleFor(x => x.Priority)
                    .Must(x => Priority.Convert(x) != null)
                    .WithMessage("Invalid Priority.");
            });

            When(x => x.DueDate != null, () => {
                RuleFor(x => x.DueDate)
                    .Must(x => DateOnly.TryParse(x.ToString(), out _))
                    .WithMessage("Invalid DueDate.");
            });

            When(x => x.MoreDetails != null, () => {
                RuleFor(x => x.MoreDetails)
                    .MaximumLength(500)
                    .WithMessage("MoreDetails cannot exceed 500 characters.");
            });
        }
    }
}