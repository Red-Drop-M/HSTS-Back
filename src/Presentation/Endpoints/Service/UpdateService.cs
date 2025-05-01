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
            AllowAnonymous();
            Description(x => x
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status200OK));
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