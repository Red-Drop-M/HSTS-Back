using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.PledgeManagement.Queries;
using Shared.Exceptions;

namespace Presentation.Endpoints.DonorPledges
{
    // Request model
    public class GetAllPledgesRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public string? DonorId { get; set; }
        public string? RequestId { get; set; }
        public string? BloodType { get; set; }
    }

    // Response model
    public class GetAllPledgesResponse
    {
        public List<DonorPledgeListDTO>? Pledges { get; set; }
        public int? Total { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }

    // Endpoint
    public class GetAllPledgesEndpoint : Endpoint<GetAllPledgesRequest, GetAllPledgesResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllPledgesEndpoint> _logger;

        public GetAllPledgesEndpoint(IMediator mediator, ILogger<GetAllPledgesEndpoint> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/donors-pledges");
            AllowAnonymous();
            Description(x => x
                .WithName("GetAllPledges")
                .Produces<GetAllPledgesResponse>(200)
                .Produces<NotFoundException>(404)
                .Produces<BadRequestException>(400));
        }

        public override async Task HandleAsync(GetAllPledgesRequest req, CancellationToken ct)
        {
            var query = new GetAllPledgesQuery(
                req.Page,
                req.PageSize,
                req.Status,
                req.DonorId,
                req.RequestId,
                req.BloodType);

            var (pledges, total, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                _logger.LogError("GetAllPledgesHandler returned error: {Error}", err.Message);
                throw err;
            }

            if (pledges == null || pledges.Count == 0)
            {
                _logger.LogWarning("No pledges found");
                throw new NotFoundException("No pledges found", "pledges_not_found");
            }

            _logger.LogInformation("Fetched {Count} pledges", pledges.Count);

            var response = new GetAllPledgesResponse
            {
                Pledges = pledges,
                Total = total,
                Message = "Pledges fetched successfully",
                StatusCode = 200
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}