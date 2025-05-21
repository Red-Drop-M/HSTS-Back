using FastEndpoints;
using Application.Features.ServiceManagement.Commands;
using Application.DTOs;
using MediatR;

namespace Presentation.Endpoints.Service
{
    public class DeleteService : Endpoint<DeleteServiceRequest, DeleteServiceResponse>
    {
        private readonly ILogger<DeleteService> _logger;
        private readonly IMediator _mediator;

        public DeleteService(ILogger<DeleteService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/services/{id}");
            Policies("RequireAdminRole"); // Restrict to admin role
            Description(x => x
                .WithName("DeleteService")
                .WithTags("Admin", "Services") // Add Admin tag
                .Produces<DeleteServiceResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status403Forbidden) // Add forbidden response
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }
        public override async Task HandleAsync(DeleteServiceRequest req, CancellationToken ct)
        {
            var command = new DeleteServiceCommand(req.Id);
            var (result, err) = await _mediator.Send(command, ct);
            if (err != null)
            {
                _logger.LogError("DeleteServiceHandler returned error: {Error}", err);
                throw err;
            }
            await SendAsync(new DeleteServiceResponse(result, 204, "Service deleted successfully"), cancellation: ct);
        }
    }

    public class DeleteServiceRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteServiceResponse
    {
        public ServiceDTO Service { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status204NoContent;
        public string Message { get; set; } = "Service deleted successfully";
        public DeleteServiceResponse(ServiceDTO service, int statusCode, string message)
        {
            Service = service;
            StatusCode = statusCode;
            Message = message;
        }
    }
}