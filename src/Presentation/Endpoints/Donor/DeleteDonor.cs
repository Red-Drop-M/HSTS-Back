using FastEndpoints;
using Application.Features.DonorManagement.Commands;
using Application.DTOs;
using MediatR;

namespace Presentation.Endpoints.Donor
{
    public class DeleteDonor : Endpoint<DeleteDonorRequest, DeleteDonorResponse>
    {
        private readonly ILogger<DeleteDonor> _logger;
        private readonly IMediator _mediator;

        public DeleteDonor(ILogger<DeleteDonor> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Delete("/donors/{id}");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("DeleteDonor")
                .WithTags("Donors")
                .Produces<DeleteDonorResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }
        public override async Task HandleAsync(DeleteDonorRequest req, CancellationToken ct)
        {
            var command = new DeleteDonorCommand(req.Id);
            var (result, err) = await _mediator.Send(command, ct);
            if (err != null)
            {
                _logger.LogError("DeleteDonorHandler returned error: {Error}", err);
                throw err;
            }
            await SendAsync(new DeleteDonorResponse(result, 204, "donor deleted successfully"), cancellation: ct);
        }
    }
    public class DeleteDonorRequest
    {
        public Guid Id { get; set; }
    }
    public class DeleteDonorResponse
    {
        public DonorDTO Donor { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status204NoContent;
        public string Message { get; set; } = "Donor deleted successfully";
        public DeleteDonorResponse(DonorDTO donor, int statusCode, string message)
        {
            Donor = donor;
            StatusCode = statusCode;
            Message = message;
        }
    }
}