using FastEndpoints;
using Application.DTOs;
using MediatR;
using Application.Features.BloodBagManagement.Queries;

namespace Presentation.Endpoints.BloodBag
{
    public class GetBloodBag : Endpoint<GetBloodBagRequest, GetBloodBagResponse>
    {
        private readonly ILogger<GetBloodBag> _logger;
        private readonly IMediator _mediator;

        public GetBloodBag(ILogger<GetBloodBag> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/blood-bags/{id}");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("GetBloodBag")
                .WithTags("BloodBags")
                .Produces<GetBloodBagResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetBloodBagRequest req, CancellationToken ct)
        {
            var query = new GetBloodBagByIdQuery(req.Id);
            var (result, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                _logger.LogError("GetBloodBagHandler returned error: {Error}", err);
                throw err;
            }

            await SendAsync(new GetBloodBagResponse(result, 200, "Blood bag fetched successfully"), cancellation: ct);
        }
    }

    public class GetBloodBagRequest
    {
        public Guid Id { get; set; }
    }

    public class GetBloodBagResponse
    {
        public BloodBagDTO BloodBag { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; } = "Blood bag retrieved successfully";
        public GetBloodBagResponse(BloodBagDTO bloodBag, int statusCode, string message)
        {
            BloodBag = bloodBag;
            StatusCode = statusCode;
            Message = message;
        }
    }
}