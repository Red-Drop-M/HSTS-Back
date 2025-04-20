using FastEndpoints;
using Application.Features.BloodRequests.Commands;
using Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
namespace Presnentation.Endpoints.BloodRequests
{
    public class DeleteRequest : Endpoint<DeleteRequestRequest, DeleteRequestResponse>
    {
        private readonly ILogger<DeleteRequest> _logger;
        private readonly IMediator _mediator;

        public DeleteRequest(ILogger<DeleteRequest> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/bloodrequests/{id}");
            AllowAnonymous();
            Description(x => x
                .Produces<DeleteRequestResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }
        public override async Task HandleAsync(DeleteRequestRequest req, CancellationToken ct)
        {
      
                var command = new DeleteRequestCommand(req.Id);
                var (result,err) = await _mediator.Send(command, ct);
                if (err != null)
                {
                    _logger.LogError("DeleteRequestHandler returned error: {Error}", err);
                    throw err;
                }
                await SendAsync(new DeleteRequestResponse(result, 204, "request deleted successfully"), cancellation: ct);
            

        }
    }
    public class DeleteRequestRequest
    {
        public Guid Id { get; set; }
    }
    public class DeleteRequestResponse
    {
        public RequestDto Request { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status204NoContent;
        public string Message { get; set; } = "Request deleted successfully";
        public DeleteRequestResponse(RequestDto request, int statusCode, string message)
        {
            Request = request;
            StatusCode = statusCode;
            Message = message;
        }
    }     

}