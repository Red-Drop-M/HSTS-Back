using FastEndpoints;
using FluentValidation;
using Domain.ValueObjects;

namespace Presentation.Endpoints.BloodBag.Validators
{
    public class UpdateBloodBagValidator : Validator<UpdateBloodBagRequest>
    {
        public UpdateBloodBagValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");

            RuleFor(x => x.BloodBagType)
                .Must(x => x != null)
                .WithMessage("Invalid BloodBagType.");

            RuleFor(x => x.ExpirationDate)
                .NotEmpty().WithMessage("ExpirationDate is required.")
                .Must(x => DateOnly.TryParse(x.ToString(), out _))
                .WithMessage("Invalid ExpirationDate.");

            RuleFor(x => x.AcquiredDate)
                .NotEmpty().WithMessage("AquieredDate is required.")
                .Must(x => DateOnly.TryParse(x.ToString(), out _))
                .WithMessage("Invalid AquieredDate.");
                
            // Add this new validation rule for RequestId
            When(x => x.RequestId.HasValue, () => {
                RuleFor(x => x.RequestId)
                    .Must(id => id != null && Guid.TryParse(id.ToString(), out _))
                    .WithMessage("Invalid RequestId.");
            });
        }
    }
}