using MediatR;
using Application.DTOs;
using FastEndpoints;
using Application.Features.DonorManagement.Queries;

namespace Presentation.Endpoints.Donor
{
    public class GetDonor : Endpoint<GetDonorRequest, GetDonorResponse>
    {
        private readonly ILogger<GetDonor> _logger;
        private readonly IMediator _mediator;

        public GetDonor(ILogger<GetDonor> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/donors/{id}");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("GetDonor")
                .WithTags("Donors")
                .Produces<GetDonorResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetDonorRequest req, CancellationToken ct)
        {
            var query = new GetDonorByIdQuery(req.Id);
            var (result, err) = await _mediator.Send(query, ct);

            if (err != null)
            {
                _logger.LogError("GetDonorHandler returned error: {Error}", err);
                throw err;
            }

            await SendAsync(new GetDonorResponse(result, 200, "Donor fetched successfully"), cancellation: ct);
        }
    }
    public class GetDonorRequest
    {
        public Guid Id { get; set; }
    }
    public class GetDonorResponse
    {
        public DonorDTO Donor { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; } = "Donor retrieved successfully";
        public GetDonorResponse(DonorDTO donor, int statusCode, string message)
        {
            Donor = donor;
            StatusCode = statusCode;
            Message = message;
        }
    }
}