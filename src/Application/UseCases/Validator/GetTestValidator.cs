using FluentValidation;

public class GetTestValidator : AbstractValidator<GetTestQuery>
{
    public GetTestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
