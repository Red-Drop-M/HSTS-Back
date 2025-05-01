using FluentValidation;
using Application.Features.DonorManagement.Commands;

namespace Application.Features.DonorManagement.Validators
{
    public class CreateDonorCommandValidator : AbstractValidator<CreateDonorCommand>
    {
        public CreateDonorCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format.");

            RuleFor(x => x.BloodType)
                .NotEmpty().WithMessage("Blood type is required.")
                .Must(bloodType => BeAValidBloodType(bloodType.ToString())).WithMessage("Invalid blood type.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(date => date < DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("Date of birth must be in the past.");

        }

        private bool BeAValidBloodType(string bloodType)
        {
            var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return validBloodTypes.Contains(bloodType);
        }
    }
}