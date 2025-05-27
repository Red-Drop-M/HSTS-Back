using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.BloodRequests.Commands;
using Domain.Repositories;
using Shared.Exceptions;

namespace Presentation.Endpoints.BloodRequests
{
    public class AutoResolveRequestRequest
    {
        public string id { get; set; }
    }

    public class AutoResolveRequestResponse
    {
        public bool success { get; set; }
        public string message { get; set; }
    }

    public class AutoResolveRequest : Endpoint<AutoResolveRequestRequest, AutoResolveRequestResponse>
    {
        private readonly ILogger<AutoResolveRequest> _logger;
        private readonly IMediator _mediator;

        public AutoResolveRequest(ILogger<AutoResolveRequest> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/bloodrequests/auto-resolve/{id}");
            AllowAnonymous(); // Or apply proper authentication
            Description(x => x
                .WithName("AutoResolveRequest")
                .WithTags("BloodRequests")
                .Produces<AutoResolveRequestResponse>(StatusCodes.Status200OK)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest));
        }

        public override async Task HandleAsync(AutoResolveRequestRequest req, CancellationToken ct)
        {
            try
            {
                var parsedId = Guid.Parse(req.id);
                var command = new AutoResolveRequestCommand(parsedId);
                var result = await _mediator.Send(command, ct);

                if (result.err != null)
                {
                    _logger.LogError("Error auto-resolving request: {Error}", result.err.Message);
                    ThrowError(result.err.Message);
                    return;
                }

                _logger.LogInformation("Successfully auto-resolved request {RequestId}", parsedId);
                await SendAsync(new AutoResolveRequestResponse
                {
                    success = true,
                    message = "Request has been successfully auto-resolved"
                }, cancellation: ct);
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid request ID format: {Id}", req.id);
                ThrowError("Invalid request ID format");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error auto-resolving request {Id}", req.id);
                ThrowError("An unexpected error occurred");
            }
        }
    }
}