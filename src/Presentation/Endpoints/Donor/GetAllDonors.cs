using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.DonorManagement.Queries;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Presentation.Endpoints.Donor
{
    public class GetAllDonorsRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class GetAllDonorsResponse
    {
        public List<DonorDTO>? Donors { get; set; }
        public int? Total { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
    public class GetAllDonors : Endpoint<GetAllDonorsRequest, GetAllDonorsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllDonors> _logger;

        public GetAllDonors(IMediator mediator, ILogger<GetAllDonors> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/donors");
            AllowAnonymous();
            Description(x => x
                .WithName("GetAllDonors")
                .Produces<GetAllDonorsResponse>(200)
                .Produces<NotFoundException>(404)
                .Produces<BadRequestException>(400));
        }
        public override async Task HandleAsync(GetAllDonorsRequest req, CancellationToken ct)
        {
            var query = new GetAllDonorsQuery(
                req.Page, 
                req.PageSize);
                
            var (donor,total, err) = await _mediator.Send(query, ct);
            if (err != null)
            {
                _logger.LogError("GetAllDonorsHandler returned error: {Error}", err);
                throw err;
            }
            if(donor == null || donor.Count == 0)
            {
                _logger.LogWarning("No donors found");
                throw new NotFoundException("No donors found", "donors_not_found");
            }
            _logger.LogInformation("Fetched {Count} donors", donor.Count);

            var response = new GetAllDonorsResponse
            {
                Donors = donor,
                Total = total,
                Message = "Donors fetched successfully",
                StatusCode = 200
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}