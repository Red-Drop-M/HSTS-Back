using FastEndpoints;
using Domain.ValueObjects;
using Presentation.Endpoints.BloodBag;
using FluentValidation;
public class CreateBloodBagValidator : Validator<CreateBloodBagRequest>
{
    public CreateBloodBagValidator()
    {
        RuleFor(x => x.BloodType)   // Change from BloodGroup to BloodType
            .NotEmpty()
            .WithMessage("Blood type is required.");
            
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

        RuleFor(x => x.AcquiredDate)
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