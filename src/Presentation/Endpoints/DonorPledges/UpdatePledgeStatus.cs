using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.PledgeManagement.Commands;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Presentation.Endpoints.DonorPledges
{
    public class UpdatePledgeStatus : Endpoint<UpdatePledgeStatusRequest, UpdatePledgeStatusResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePledgeStatus> _logger;

        public UpdatePledgeStatus(IMediator mediator, ILogger<UpdatePledgeStatus> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Put("/donors-pledges/{donorId}/{requestId}/status");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("UpdatePledgeStatus")
                .WithTags("DonorPledges")
                .Produces<UpdatePledgeStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(UpdatePledgeStatusRequest req, CancellationToken ct)
        {
            try
            {
                var command = new UpdatePledgeStatusCommand(
                    req.DonorId,
                    req.RequestId,
                    PledgeStatus.FromString(req.Status)
                );

                var (result, error) = await _mediator.Send(command, ct);

                if (error != null)
                {
                    _logger.LogWarning("Failed to update pledge status: {Message}", error.Message);
                    throw error;
                }

                _logger.LogInformation("Pledge status updated successfully for donor {DonorId} and request {RequestId}", 
                    req.DonorId, req.RequestId);
                
                await SendAsync(new UpdatePledgeStatusResponse
                {
                    DonorId = result.DonorId,
                    RequestId = result.RequestId,
                    Status = result.Status,
                    Message = "Pledge status updated successfully",
                    Success = true
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error updating pledge status for donor {DonorId} and request {RequestId}", 
                    req.DonorId, req.RequestId);
                throw new InternalServerException("An error occurred while updating the pledge status", "update-pledge-status");
            }
        }
    }

    public class UpdatePledgeStatusRequest
    {
        [BindFrom("donorId")]
        public Guid DonorId { get; set; }

        [BindFrom("requestId")]
        public Guid RequestId { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePledgeStatusResponse
    {
        public Guid DonorId { get; set; }
        public Guid RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}