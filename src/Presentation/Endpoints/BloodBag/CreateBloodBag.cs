using MediatR;
using FastEndpoints;
using Domain.ValueObjects;
using Application.Features.BloodBagManagement.Commands; 
using Shared.Exceptions;
using Application.DTOs;

namespace Presentation.Endpoints.BloodBag
{
    public class CreateBloodBag : Endpoint<CreateBloodBagRequest, CreateBloodBagResponse>
    {
        private readonly ILogger<CreateBloodBag> _logger;
        private readonly IMediator _mediator;

        public CreateBloodBag(ILogger<CreateBloodBag> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/blood-bags");
            AllowAnonymous();//Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("CreateBloodBag")
                .WithTags("BloodBags")
                .Produces<CreateBloodBagResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateBloodBagRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateBloodBagCommand(
                    BloodType.FromString(req.BloodType),
                    BloodBagType.Convert(req.BloodBagType),
                    req.ExpirationDate,
                    req.AcquiredDate,
                    req.DonorId,
                    req.RequestId,
                    req.Status != null ? Domain.ValueObjects.BloodBagStatus.Convert(req.Status) : null
                );

                var result = await _mediator.Send(command, ct);

                if (result.bloodBag == null || result.err != null)
                {
                    _logger.LogError("CreateBloodBagHandler returned error: {Error}", 
                        result.err?.Message ?? "Unknown error");
                    throw result.err != null 
                        ? result.err 
                        : new InvalidOperationException("CreateBloodBagHandler returned null result");
                }

                _logger.LogInformation("Blood bag created successfully with ID: {Id}", result.bloodBag.Id);
                var response = new CreateBloodBagResponse
                {
                    content = result.bloodBag,
                    success = true,
                    Error = null
                };
                await SendAsync(response, cancellation: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Input: {ErrorMessage}", ex.Message);
                throw new ValidationException($"Error creating blood bag: {ex.Message}", "create_blood_bag");
            }
        }
    }

    public class CreateBloodBagRequest
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
        public string? Status { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AcquiredDate { get; set; }
        public required Guid DonorId { get; set; }
        public Guid? RequestId { get; set; }
    }

    public class CreateBloodBagResponse
    {
        public required BloodBagDTO content { get; set; }
        public bool? success { get; set; }
        public string? Error { get; set; }
    }
}