namespace Presentation.Endpoints.TestWithId.Validators
{
    using Application.Features.Test.Queries;
    using FluentValidation;
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestWithIdValidator : AbstractValidator<TestWithIdQuery>
    {
        public TestWithIdValidator()
        {
            RuleFor(x => x.Id).NotNull().WithMessage("Id cannot be null.");
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}