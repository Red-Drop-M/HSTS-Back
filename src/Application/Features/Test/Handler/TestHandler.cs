using MediatR;
using Application.Features.Test.Queries;
namespace Application.Features.Test.Handler
{
    public class TestHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult("Hello, World from Query using FastEndpoints! + MediatR");
        }
    }
}