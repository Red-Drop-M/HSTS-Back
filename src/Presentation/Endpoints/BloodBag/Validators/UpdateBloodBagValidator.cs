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

            RuleFor(x => x.BloodType)
                .Must(x => x != null)
                .WithMessage("Invalid BloodType.");

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

            RuleFor(x => x.DonorId)
                .NotEmpty().WithMessage("DonorId is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid DonorId.");

            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("RequestId is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid RequestId.");
        }
    }
}