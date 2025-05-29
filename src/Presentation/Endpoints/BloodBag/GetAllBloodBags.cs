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
            public string? BloodBagType { get; set; }
            public string? BloodType { get; set; }
            public string? Status { get; set; }
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
             AllowAnonymous();//Roles("Admin", "User"); // Both admins and regular users can access
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
            // Convert string values to domain value objects
            BloodBagType? bloodBagType = null;
            BloodType? bloodType = null;
            BloodBagStatus? status = null;
            
            if (!string.IsNullOrEmpty(req.BloodBagType))
            {
                try {
                    bloodBagType = Domain.ValueObjects.BloodBagType.Convert(req.BloodBagType);
                }
                catch (ArgumentException) {
                    AddError(nameof(req.BloodBagType), "Invalid blood bag type");
                }
            }
            
            if (!string.IsNullOrEmpty(req.BloodType))
            {
                try {
                    bloodType = Domain.ValueObjects.BloodType.FromString(req.BloodType);
                }
                catch (ArgumentException) {
                    AddError(nameof(req.BloodType), "Invalid blood type");
                }
            }
            
            if (!string.IsNullOrEmpty(req.Status))
            {
                try {
                    status = Domain.ValueObjects.BloodBagStatus.Convert(req.Status);
                }
                catch (ArgumentException) {
                    AddError(nameof(req.Status), "Invalid status");
                }
            }
            
            if (ValidationFailed)
            {
                await SendErrorsAsync(400, ct);
                return;
            }

            var query = new GetAllBloodBagsQuery(
                req.PageNumber,
                req.PageSize, 
                bloodBagType,
                bloodType, 
                status, 
                req.ExpirationDate, 
                req.AcquiredDate, 
                req.DonorId, 
                req.ServiceId);
                
            var (request, total, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                // Check if this is a "No blood bags found" NotFoundException
                if (err is NotFoundException notFoundEx && notFoundEx.Message.Contains("No blood bags found"))
                {
                    _logger.LogWarning("No blood bags found.");
                    await SendAsync(new GetAllBloodBagsResponse
                    {
                        BloodBags = new List<BloodBagDTO>(),
                        Total = 0,
                        StatusCode = 200,
                        Message = "No blood bags found matching the criteria"
                    }, cancellation: ct);
                    return;
                }
                
                // Otherwise, it's a different error that should be thrown
                _logger.LogError("GetAllBloodBagsHandler returned error: {Error}", err);
                throw err;
            }

            // Return empty list with 200 instead of throwing 404
            if (request == null || request.Count == 0)
            {
                _logger.LogWarning("No blood bags found.");
                await SendAsync(new GetAllBloodBagsResponse
                {
                    BloodBags = new List<BloodBagDTO>(),
                    Total = 0,
                    StatusCode = 200,
                    Message = "No blood bags found matching the criteria"
                }, cancellation: ct);
                return;
            }

            _logger.LogInformation("Fetched {Count} blood bags successfully.", request.Count);

            var response = new GetAllBloodBagsResponse
            {
                BloodBags = request,
                Total = total,
                StatusCode = 200,
                Message = "Blood bags fetched successfully"
            };

            await SendAsync(response, cancellation: ct);
        }

    }
}