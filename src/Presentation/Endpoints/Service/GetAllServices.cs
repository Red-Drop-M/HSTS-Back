using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.ServiceManagement.Queries;
using Shared.Exceptions;

namespace Presentation.Endpoints.Service
{
    public class GetAllServicesRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetAllServicesResponse
    {
        public List<ServiceDTO>? Services { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class GetAllServices : Endpoint<GetAllServicesRequest, GetAllServicesResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllServices> _logger;

        public GetAllServices(IMediator mediator, ILogger<GetAllServices> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/services");
            AllowAnonymous();
            Description(x => x
                .WithName("GetAllServices")
                .Produces<GetAllServicesResponse>(200)
                .Produces<NotFoundException>(404)
                .Produces<BadRequestException>(400));
        }
        public override async Task HandleAsync(GetAllServicesRequest req, CancellationToken ct)
        {
                var query = new GetAllServicesQuery(
                    req.Page, 
                    req.PageSize);

                var (services,err) = await _mediator.Send(query, ct);

                if (err is not null)
                {
                    _logger.LogError("GetAllServicesHandler returned error: {Error}", err);
                    throw err;
                }
                if (services is null || services.Count == 0)
                {
                    _logger.LogWarning("No services found for the given criteria.");
                    throw new NotFoundException("No services found for the given criteria.", "Fetching services");
                }
                _logger.LogInformation("GetAllServicesHandler success returned {result}", services);
                

                var response = new GetAllServicesResponse
                {
                    Services = services,
                    Message = "Services retrieved successfully.",
                    StatusCode = 200
                };

                await SendAsync(response, cancellation: ct);
            
        }
    }
}