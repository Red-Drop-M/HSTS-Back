using FluentValidation;
using Domain.ValueObjects;

namespace Presentation.Endpoints.Service.Validators
{
    public class GetAllServicesValidator : AbstractValidator<GetAllServicesRequest>
    {
        public GetAllServicesValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero.");
        }
    }
}