using MediatR;
using FastEndpoints;
using Domain.ValueObjects;
using Application.Features.ServiceManagement.Commands;
using Application.DTOs;

namespace Presentation.Endpoints.Service
{
    public class UpdateService : Endpoint<UpdateServiceRequest, UpdateServiceResponse>
    {
        private readonly ILogger<UpdateService> _logger;
        private readonly IMediator _mediator;

        public UpdateService(ILogger<UpdateService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/services/{id}");
            Policies("RequireAdminRole"); // Restrict to admin role
            Description(x => x
                .WithName("UpdateService")
                .WithTags("Admin", "Services") // Add Admin tag
                .Produces<UpdateServiceResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status403Forbidden) // Add forbidden response
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }
        public override async Task HandleAsync(UpdateServiceRequest req, CancellationToken ct)
        {
            var command = new UpdateServiceCommand(req.Id,req.Name);
            var (result, err) = await _mediator.Send(command, ct);

            if (err != null)
            {
                _logger.LogError("Error while updating service: {Error}", err);
                throw err;
            }

            _logger.LogInformation("Service updated successfully: {Result}", result);
            var response = new UpdateServiceResponse(result, 200, "Service updated successfully");
            await SendAsync(response, cancellation: ct);
        }
    }
    public class UpdateServiceRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class UpdateServiceResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public ServiceDTO Service { get; set; }

        public UpdateServiceResponse(ServiceDTO service, int status, string message)
        {
            Service = service;
            Status = status;
            Message = message;
        }
    }
}