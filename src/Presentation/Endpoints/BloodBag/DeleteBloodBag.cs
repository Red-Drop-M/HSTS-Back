using FastEndpoints;
using Application.Features.BloodBagManagement.Commands;
using Application.DTOs;
using MediatR;

namespace Presentation.Endpoints.BloodBag
{
    public class DeleteBloodBag : Endpoint<DeleteBloodBagRequest, DeleteBloodBagResponse>
    {
        private readonly ILogger<DeleteBloodBag> _logger;
        private readonly IMediator _mediator;

        public DeleteBloodBag(ILogger<DeleteBloodBag> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/blood-bags/{id}");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("DeleteBloodBag")
                .WithTags("BloodBags")
                .Produces<DeleteBloodBagResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(DeleteBloodBagRequest req, CancellationToken ct)
        {
            var command = new DeleteBloodBagCommand(req.Id);
            var (result, err) = await _mediator.Send(command, ct);
            if (err != null)
            {
                _logger.LogError("DeleteBloodBagHandler returned error: {Error}", err);
                throw err;
            }
            await SendAsync(new DeleteBloodBagResponse(result, 204, "Blood bag deleted successfully"), cancellation: ct);
        }
    }

    public class DeleteBloodBagRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteBloodBagResponse
    {
        public BloodBagDTO BloodBag { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status204NoContent;
        public string Message { get; set; } = "Blood bag deleted successfully";
        public DeleteBloodBagResponse(BloodBagDTO bloodBag, int statusCode, string message)
        {
            BloodBag = bloodBag;
            StatusCode = statusCode;
            Message = message;
        }
    }
}
