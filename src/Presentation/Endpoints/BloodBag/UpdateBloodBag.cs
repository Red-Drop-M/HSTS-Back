using MediatR;
using Application.DTOs;
using Application.Features.BloodBagManagement.Commands;
using Shared.Exceptions;
using FastEndpoints;
using Domain.ValueObjects;

namespace Presentation.Endpoints.BloodBag
{
    public class UpdateBloodBag : Endpoint<UpdateBloodBagRequest, UpdateBloodBagResponse>
    {
        private readonly ILogger<UpdateBloodBagRequest> _logger;
        private readonly IMediator _mediator;

        public UpdateBloodBag(ILogger<UpdateBloodBagRequest> logger,IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/bloodbags/{id}");
            AllowAnonymous();
            Description(x => x
                .WithName("Update Blood Bag")
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status200OK));
        }
        public override async Task HandleAsync(UpdateBloodBagRequest req, CancellationToken ct)
        {
            var command = new UpdateBloodBagCommand(
                req.Id, 
                req.BloodBagType, 
                req.BloodType, 
                req.Status, 
                req.ExpirationDate, 
                req.AcquiredDate,
                req.RequestId);
            var (result, err) = await _mediator.Send(command, ct);
            if (err != null)
            {
                _logger.LogError("Error while updating blood bag");
                throw err;
            }
            _logger.LogInformation("Blood bag updated successfully");
            var response = new UpdateBloodBagResponse(result, 200, "Blood bag updated successfully");
            await SendAsync(response, cancellation: ct); 
        }
    }
    public class UpdateBloodBagRequest
    {
        public required Guid Id { get; set; }
        public BloodBagType? BloodBagType { get; set; }
        public BloodType? BloodType { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; }
        public BloodBagStatus? Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? RequestId { get; set; }
    }
    public class UpdateBloodBagResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public BloodBagDTO BloodBag { get; set; }

        public UpdateBloodBagResponse(BloodBagDTO bloodBag, int status, string message)
        {
            BloodBag = bloodBag;
            Status = status;
            Message = message;
        }
    }
}