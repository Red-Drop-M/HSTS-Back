using FastEndpoints;
using Domain.ValueObjects;
using Presentation.Endpoints.BloodBag;
using FluentValidation;
public class CreateBloodBagValidator : Validator<CreateBloodBagRequest>
{
    public CreateBloodBagValidator()
    {
        RuleFor(x => x.BloodGroup)
            .NotEmpty()
            .WithMessage("Blood group is required.")
            .Must(bloodGroup => BloodType.FromString(bloodGroup) != null)
            .WithMessage("Invalid blood group.");

        RuleFor(x => x.BloodBagType)
            .NotEmpty()
            .WithMessage("Blood bag type is required.")
            .Must(bloodBagType => BloodBagType.Convert(bloodBagType) != null)
            .WithMessage("Invalid blood bag type.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .WithMessage("Expiration date is required.")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Expiration date must be today or in the future.");

        RuleFor(x => x.AquieredDate)
            .NotEmpty()
            .WithMessage("Aquired date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Aquired date cannot be in the future.");

        RuleFor(x => x.DonorId)
            .Must(donorId => donorId == null || Guid.TryParse(donorId.ToString(), out _))
            .WithMessage("Invalid Donor ID.");

        RuleFor(x => x.RequestId)
            .Must(requestId => requestId == null || Guid.TryParse(requestId.ToString(), out _))
            .WithMessage("Invalid Request ID.");
    }
}