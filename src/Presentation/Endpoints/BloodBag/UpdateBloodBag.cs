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
            Put("/blood-bags/{id}");
            AllowAnonymous(); // Uncomment this line to allow anonymous access
            //Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("UpdateBloodBag")
                .WithTags("BloodBags")
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status200OK));
        }
        public override async Task HandleAsync(UpdateBloodBagRequest req, CancellationToken ct)
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
            
            ThrowIfAnyErrors();

            var command = new UpdateBloodBagCommand(
                req.Id, 
                bloodType,
                bloodBagType, 
                status, 
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
        public string? BloodBagType { get; set; }  
        public string? BloodType { get; set; }     
        public string? Status { get; set; }        
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; } // Use standard spelling, not "AquieredDate"
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