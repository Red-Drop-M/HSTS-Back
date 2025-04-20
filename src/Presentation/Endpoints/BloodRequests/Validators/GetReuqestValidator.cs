using FastEndpoints;
using FluentValidation;
using Presentation.Endpoints.BloodRequests;
using Domain.ValueObjects;
namespace Presentation.Endpoints.BloodRequests.Validators
{
    public class GetRequestValidator : Validator<GetRequestRequest>
    {
        public GetRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .Must(id => Guid.TryParse(id.ToString(), out _))
                .WithMessage("Invalid Id.");
        }
    }
}