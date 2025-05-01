using MediatR;
using FastEndpoints;
using Application.Features.ServiceManagement.Commands;
using Shared.Exceptions;
using Application.DTOs;

namespace Presentation.Endpoints.Service
{
    public class CreateService : Endpoint<CreateServiceRequest, CreateServiceResponse>
    {
        private readonly ILogger<CreateService> _logger;
        private readonly IMediator _mediator;

        public CreateService(ILogger<CreateService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/services");
            AllowAnonymous();
            Description(x => x
                .Produces<CreateServiceResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateServiceRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateServiceCommand(
                    req.Name
                );

                var result = await _mediator.Send(command, ct);

                if (result.service == null)
                {
                    _logger.LogError("CreateServiceHandler returned null");
                    throw new InvalidOperationException("CreateServiceHandler returned null: CreateService");
                }

                _logger.LogInformation("CreateServiceHandler success returned {result}", result);
                var response = new CreateServiceResponse
                {
                    content = result.service,
                    success = true,
                    Error = null
                };
                await SendAsync(response, cancellation: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service: {Message}", ex.Message);
                throw new ValidationException(ex.Message, "create_service");
            }
        }
    }

    public class CreateServiceRequest
    {
        public required string Name { get; set; }
    }
    public class CreateServiceResponse
    {
        public ServiceDTO? content { get; set; }
        public bool? success { get; set; }
        public string? Error { get; set; }
    }
}

