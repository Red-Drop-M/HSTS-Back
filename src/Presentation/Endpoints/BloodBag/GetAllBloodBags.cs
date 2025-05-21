using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.BloodBagManagement.Queries;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Presentation.Endpoints.BloodBag
{
        public class GetAllBloodBagsRequest
        {
            public BloodBagType? BloodBagType { get; set; }
            public BloodType? BloodType { get; set; }
            public BloodBagStatus? Status { get; set; }
            public DateOnly? ExpirationDate { get; set; }
            public DateOnly? AcquiredDate { get; set; }
            public Guid? DonorId { get; set; }
            public Guid? ServiceId { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;

        }

        public class GetAllBloodBagsResponse
        {
            public List<BloodBagDTO>? BloodBags { get; set; } 
            public int? Total { get; set; }
            public string? Message { get; set; } 
            public int StatusCode { get; set; } 
        }


    public class GetAllBloodBags 
        : Endpoint<GetAllBloodBagsRequest, GetAllBloodBagsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllBloodBags> _logger;

        public GetAllBloodBags(IMediator mediator, ILogger<GetAllBloodBags> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/blood-bags");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("GetAllBloodBags")
                .WithTags("BloodBags")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetAllBloodBagsRequest req, CancellationToken ct)
        {
            var query = new GetAllBloodBagsQuery(
                req.PageNumber,
                req.PageSize, 
                req.BloodBagType,
                req.BloodType, 
                req.Status, 
                req.ExpirationDate, 
                req.AcquiredDate, 
                req.DonorId, 
                req.ServiceId);
            var (request, total, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                _logger.LogError("GetAllBloodBagsHandler returned error: {Error}", err);
                throw err;
            }

            if (request == null || request.Count == 0)
            {
                _logger.LogWarning("No blood bags found.");
                throw new NotFoundException("No blood bags found.","Fetch Blood Bags");
            }
            _logger.LogInformation("Fetched {Count} blood bags successfully.", request.Count);

            var response = new GetAllBloodBagsResponse
            {
                BloodBags = request,
                Total     = total,
                StatusCode = 200,
                Message    = "Blood bags fetched successfully"
            };

            await SendAsync(response, cancellation: ct);

        }

    }
}