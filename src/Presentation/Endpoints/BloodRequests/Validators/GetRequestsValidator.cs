using System;
using FluentValidation;
using Presentation.Endpoints.BloodRequests;
using Domain.ValueObjects;

namespace Presentation.Endpoints.BloodRequests.Validators
{
    public class GetRequestsRequestValidator : AbstractValidator<GetRequestsRequest>
    {
        public GetRequestsRequestValidator()
        {
            // Pagination
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero.");

            // Date filters
            RuleFor(x => x.RequestDate)
                .Must(BeAValidDate)
                .When(x => !string.IsNullOrEmpty(x.RequestDate))
                .WithMessage("RequestDate must be a valid date (yyyy-MM-dd).");

            RuleFor(x => x.DueDate)
                .Must(BeAValidDate)
                .When(x => !string.IsNullOrEmpty(x.DueDate))
                .WithMessage("DueDate must be a valid date (yyyy-MM-dd).");

            // GUID filters
            RuleFor(x => x.DonorId)
                .Must(BeAValidGuid)
                .When(x => !string.IsNullOrEmpty(x.DonorId))
                .WithMessage("DonorId must be a valid GUID.");

            RuleFor(x => x.ServiceId)
                .Must(BeAValidGuid)
                .When(x => !string.IsNullOrEmpty(x.ServiceId))
                .WithMessage("ServiceId must be a valid GUID.");

            // ValueObject filters
            RuleFor(x => x.Priority)
                .Must(BeAValidPriority)
                .When(x => !string.IsNullOrEmpty(x.Priority))
                .WithMessage("Priority is invalid.");

            RuleFor(x => x.BloodBagType)
                .Must(BeAValidBloodBagType)
                .When(x => !string.IsNullOrEmpty(x.BloodBagType))
                .WithMessage("BloodBagType is invalid.");

            RuleFor(x => x.Status)
                .Must(BeAValidStatus)
                .When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Status is invalid.");

            RuleFor(x => x.BloodGroup)
                .Must(BeAValidBloodGroup)
                .When(x => !string.IsNullOrEmpty(x.BloodGroup))
                .WithMessage("BloodGroup is invalid.");
        }

        private bool BeAValidDate(string? date)
            => !string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out _);

        private bool BeAValidGuid(string? id)
            => !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out _);

        private bool BeAValidPriority(string? p)
            => !string.IsNullOrWhiteSpace(p) && TryConvert(() => Priority.Convert(p!));

        private bool BeAValidBloodBagType(string? bt)
            => !string.IsNullOrWhiteSpace(bt) && TryConvert(() => BloodBagType.Convert(bt!));

        private bool BeAValidStatus(string? s)
            => !string.IsNullOrWhiteSpace(s) && TryConvert(() => RequestStatus.Convert(s!));

        private bool BeAValidBloodGroup(string? bg)
            => !string.IsNullOrWhiteSpace(bg) && TryConvert(() => BloodType.FromString(bg!));

        private bool TryConvert(Func<object> convert)
        {
            try { convert(); return true; }
            catch { return false; }
        }
    }
}