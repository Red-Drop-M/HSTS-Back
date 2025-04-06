using MediatR;
namespace Application.Features.Test.Queries
{
    public record TestQuery() : IRequest<string>;
}