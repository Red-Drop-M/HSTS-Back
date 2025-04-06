using Application.Features.Test.Queries;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
namespace Presentation.Endpoints.TestWithId
{
    public class TestWithId:Endpoint<TestWithIdRequest, TestWithIdResponse>
    {
        private readonly ILogger<TestWithId> _logger;
        private readonly IMediator _mediator;
        public TestWithId(ILogger<TestWithId> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        public override void Configure()
        {
            Get("/test/{id}");
            AllowAnonymous();
            Description(x => x
                .Produces<TestWithIdResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(TestWithIdRequest req, CancellationToken ct)
        {
            var result = await _mediator.Send(new TestWithIdQuery(req.Id), ct);
                if (result == null)
            {
                _logger.LogError("TestWithIdHandler returned null");
                await SendNotFoundAsync(ct);
                return;
            }
            _logger.LogInformation("TestWithIdHandler success returned {result}", result);
            var response = new TestWithIdResponse
            {
                Message = result
            };
            await SendAsync(response, cancellation: ct);
        }
    }

    public class TestWithIdRequest : IRequest<string>
    {
        public int Id { get; set; }
    }
    public class TestWithIdResponse
    {
        public required string Message { get; set; }
    }
    
}