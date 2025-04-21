using FluentValidation;
using Application.Features.BloodBagManagement.Commands;
using Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace Application.Features.BloodBagManagement.Validators
{
    public class CreateBloodBagCommandValidator : AbstractValidator<CreateBloodBagCommand>
    {
        public CreateBloodBagCommandValidator()
        {
 
            RuleFor(x => x.BloodType)
                .NotEmpty().WithMessage("Blood type is required.")
                .Must(bloodType => BeAValidBloodType(bloodType.ToString())).WithMessage("Invalid blood type.");

            RuleFor(x => x.DonorId)
                .NotEmpty().WithMessage("Donor ID is required.");
        }

        private bool BeAValidBloodType(string bloodType)
        {
            var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return validBloodTypes.Contains(bloodType);
        }
    }
}