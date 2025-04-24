using FluentValidation;
using Domain.ValueObjects;
using Presentation.Endpoints.BloodBag;

namespace Presentation.Endpoints.BloodBag.Validators
{
    public class GetAllBloodBagRequestValidator : AbstractValidator<GetAllBloodBagsRequest>
    {
        public GetAllBloodBagRequestValidator()
        {
            // Pagination
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page must be greater than zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero.");

            // Date filters
            RuleFor(x => x.ExpirationDate.ToString())
                .Must(BeAValidDate)
                .When(x => x.ExpirationDate.HasValue)
                .WithMessage("ExpirationDate must be a valid date (yyyy-MM-dd).");

            RuleFor(x => x.AcquiredDate.ToString())
                .Must(BeAValidDate)
                .When(x => x.AcquiredDate.HasValue)
                .WithMessage("AquiredDate must be a valid date (yyyy-MM-dd).");

            // GUID filters
            RuleFor(x => x.DonorId)
                .Must(id => id.HasValue)
                .When(x => x.DonorId.HasValue)
                .WithMessage("DonorId must be a valid GUID.");

            RuleFor(x => x.ServiceId)
                .Must(id => id.HasValue)
                .When(x => x.ServiceId.HasValue)
                .WithMessage("ServiceId must be a valid GUID.");

            // ValueObject filters
            RuleFor(x => x.BloodBagType)
                .Must(type => type != null)
                .When(x => x.BloodBagType != null)
                .WithMessage("BloodBagType is invalid.");

            RuleFor(x => x.BloodType)
                .Must(type => type != null)
                .When(x => x.BloodType != null)
                .WithMessage("BloodType is invalid.");
        }
        private bool BeAValidDate(string? date)
            => DateOnly.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);

        private bool BeAValidGuid(string id)
            => !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out _);

        private bool BeAValidBloodBagType(string? bt)
            => !string.IsNullOrWhiteSpace(bt) && TryConvertBloodBagType(() => BloodBagType.Convert(bt!));

        private bool BeAValidBloodType(string bloodType)
            => !string.IsNullOrWhiteSpace(bloodType) && TryConvertBloodType(() => BloodType.FromString(bloodType!));

        private bool TryConvertBloodBagType(Func<BloodBagType> convert)
        {
            try { convert(); return true; }
            catch { return false; }
        }

        private bool TryConvertBloodType(Func<BloodType> convert)
        {
            try{ convert(); return true; }
            catch{ return false;}
        }
    }
}