using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.DTOs;
using Application.Features.BloodRequests.Queries;
using Shared.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation.Endpoints.BloodRequests
{
    // 1) The input model bound from query‐string
    public class GetRequestsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Priority { get; set; }
        public string? BloodBagType { get; set; }
        public string? RequestDate { get; set; }
        public string? DueDate { get; set; }
        public string? DonorId { get; set; }
        public string? ServiceId { get; set; }
        public string? Status { get; set; }
        public string? BloodGroup { get; set; }
    }

    // 2) The shaped response model
    public class GetRequestsResponse
    {
        public List<RequestDto>? Requests { get; set; }
        public int? Total { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }

    // 3) The endpoint itself
    public class GetRequestsEndpoint 
        : Endpoint<GetRequestsRequest, GetRequestsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetRequestsEndpoint> _logger;

        public GetRequestsEndpoint(IMediator mediator, ILogger<GetRequestsEndpoint> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/bloodrequests");
            AllowAnonymous();
            Description(x => x
                .WithName("GetBloodRequests")
                .Produces<GetRequestsResponse>(200)
                .Produces<NotFoundException>(404)
                .Produces<BadRequestException>(400));
        }

        public override async Task HandleAsync(GetRequestsRequest req, CancellationToken ct)
        {
            // 4) Map your FastEndpoints DTO → MediatR query
            var query = new GetRequestsQuery(
                req.Page,
                req.PageSize,
                req.Priority,
                req.BloodBagType,
                req.RequestDate,
                req.DueDate,
                req.DonorId,
                req.ServiceId,
                req.Status,
                req.BloodGroup
            );

            // 5) Send through MediatR
            var (requests, total, err) = await _mediator.Send(query, ct);

            if (err is not null)
            {
                _logger.LogError("GetRequestsHandler returned error: {Error}", err.Message);
                throw err;
            }
            if (requests is null || requests.Count == 0)
            {
                _logger.LogWarning("No blood requests found");
                throw new NotFoundException("No blood requests found", "Fetching blood requests");
            }
            _logger.LogInformation("Fetched {Count} blood requests", requests.Count);

            // 6) Shape your response
            var response = new GetRequestsResponse
            {
                Requests   = requests,
                Total      = total,
                StatusCode = 200,
                Message    = "Blood requests fetched successfully"
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
