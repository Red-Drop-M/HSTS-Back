using FastEndpoints;
using Application.DTOs;
using MediatR;
using Application.Features.ServiceManagement.Queries;

namespace Presentation.Endpoints.Service
{
    public class GetService : Endpoint<GetServiceRequest, GetServiceResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetService> _logger;

        public GetService(IMediator mediator, ILogger<GetService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/services/{id}");
            Roles("Admin", "User"); // Correct method in FastEndpoints v5.35.0
            Description(x => x
                .WithName("GetService")
                .WithTags("Services") // Remove "Admin" tag since all users can access
                .Produces<GetServiceResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized) // Changed from 403 to 401
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetServiceRequest req, CancellationToken ct)
        {
            var query = new GetServiceByIdQuery(req.Id);
            var (result, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                _logger.LogError("GetServiceHandler returned error: {Error}", err);
                throw err;
            }

            await SendAsync(new GetServiceResponse(result, 200, "Service fetched successfully"), cancellation: ct);
        }
    }

    public class GetServiceRequest
    {
        public Guid Id { get; set; }
    }
    public class GetServiceResponse
    {
        public ServiceDTO Service { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; } = "Service retrieved successfully";
        public GetServiceResponse(ServiceDTO service, int statusCode, string message)
        {
            Service = service;
            StatusCode = statusCode;
            Message = message;
        }
    }
}