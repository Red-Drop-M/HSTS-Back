namespace Application.Features.Test.Queries
{
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;

    public record TestWithIdQuery(int Id) : IRequest<string>;
}