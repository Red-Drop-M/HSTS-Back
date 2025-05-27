using FastEndpoints;
using MediatR;
using Application.Features.BloodBagManagement.Commands;

namespace Presentation.Endpoints.BloodBag
{
    public class AssignBloodBagsRequest
    {
        public Guid RequestId { get; set; }
        public List<Guid> BloodBagIds { get; set; } = new();
    }

    public class AssignBloodBagsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int AssignedCount { get; set; }
    }

    public class AssignBloodBags : Endpoint<AssignBloodBagsRequest, AssignBloodBagsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AssignBloodBags> _logger;

        public AssignBloodBags(IMediator mediator, ILogger<AssignBloodBags> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/blood-bags/assign");
            Roles("Admin", "User");
            Description(x => x
                .WithName("AssignBloodBags")
                .WithTags("BloodBags")
                .Produces<AssignBloodBagsResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound));
        }

        public override async Task HandleAsync(AssignBloodBagsRequest req, CancellationToken ct)
        {
            var command = new AssignBloodBagsCommand(req.RequestId, req.BloodBagIds);
            var result = await _mediator.Send(command, ct);

            if (result.err != null)
            {
                _logger.LogError("Error assigning blood bags: {Error}", result.err.Message);
                ThrowError(result.err.Message);
                return;
            }

            await SendAsync(new AssignBloodBagsResponse
            {
                Success = true,
                Message = "Blood bags successfully assigned to request",
                AssignedCount = result.assignedCount
            }, cancellation: ct);
        }
    }
}