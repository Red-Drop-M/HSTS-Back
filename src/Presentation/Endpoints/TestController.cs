using MediatR;
using FastEndpoints;
using Application.Features.Test.Queries;
namespace Presentation.Controllers
{
    public class TestController: EndpointWithoutRequest<string>
    {
        private readonly IMediator _mediator;
        public TestController(IMediator mediator)
        {
            _mediator = mediator;
        }
        public override void Configure()
        {
            Get("/test");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await _mediator.Send(new TestQuery(), ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}