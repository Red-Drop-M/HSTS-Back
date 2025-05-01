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
            Post("/bloodbags");
            AllowAnonymous();
            Description(x => x
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
                    BloodType.FromString(req.BloodGroup), // Convert string to BloodType
                    BloodBagType.Convert(req.BloodBagType),
                    req.ExpirationDate,
                    req.AquieredDate,
                    req.DonorId,
                    req.RequestId
                );

                var result = await _mediator.Send(command, ct);

                if (result.bloodBag == null || result.err == null)
                {
                    _logger.LogError("CreateBloodBagHandler returned null");
                    throw new InvalidOperationException("CreateBloodBagHandler returned null: CreateBloodBag");
                }

                _logger.LogInformation("CreateBloodBagHandler success returned {result}", result);
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
                _logger.LogError(ex, "Invalid Input");
                throw new ValidationException(ex.Message,"create_blood_bag");
            }
        }
    }

    public class CreateBloodBagRequest
    {
        public required string BloodGroup { get; set; }
        public required string BloodBagType { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public DateOnly? AquieredDate { get; set; }
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