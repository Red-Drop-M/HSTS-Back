namespace Application.Features.Test.Handler
{
    using MediatR;
    using Application.Features.Test.Queries;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestWithIdHandler : IRequestHandler<TestWithIdQuery, string>
    {
        public Task<string> Handle(TestWithIdQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, World from Query with Id {request.Id} using FastEndpoints! + MediatR");
        }
    }
}