using MediatR;
using FastEndpoints;
using Domain.ValueObjects;
using Application.Features.BloodRequests.Commands;
using Shared.Exceptions;
using Application.DTOs;
namespace Presentation.Endpoints.BloodRequests
{
    public class CreateRequest : Endpoint<CreateRequestRequest, CreateRequestResponse>
    {
        private readonly ILogger<CreateRequest> _logger;
        private readonly IMediator _mediator;

        public CreateRequest(ILogger<CreateRequest> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/bloodrequests");
            AllowAnonymous();
            Description(x => x
                .Produces<CreateRequestResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateRequestRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateRequestCommand(
                    BloodType.FromString(req.BloodType),
                    BloodBagType.Convert(req.BloodBagType), 
                    Priority.Convert(req.Priority), 
                    req.DueDate, 
                    req.MoreDetails,
                    req.ServiceId,
                    req.DonorId,
                    RequestStatus.Convert(req.RequestStatus ?? "pending"), 
                    req.RequestDate,
                    req.AquiredQty,
                    req.RequiredQty
                );

                var result = await _mediator.Send(command, ct);

                if (result == null)
                {
                    _logger.LogError("CreateRequestHandler returned null");
                    throw new InvalidOperationException("CreateRequestHandler returned null: CreateRequest");
                }

                _logger.LogInformation("CreateRequestHandler success returned {result}", result);
                var response = new CreateRequestResponse()
                {
                    content = result,
                    success = true,
                    error = null
                };
                await SendAsync(response, cancellation: ct);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input");
                throw new ValidationException(ex.Message,"create_request");
            }
        }
    }

    public class CreateRequestRequest 
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
        public required string Priority { get; set; }
        public required DateOnly? DueDate { get; set; }
        public  string? MoreDetails { get; set; }
        public  Guid? ServiceId { get; set; }
        public  Guid? DonorId { get; set; }
        public required string? RequestStatus { get; set; }
        public required DateOnly? RequestDate { get; set; }
        public required int AquiredQty { get; set; }
        public required int RequiredQty { get; set; }
    }

    public class CreateRequestResponse
    {
        public required RequestDto content { get; set; }
        public required bool? success { get; set; }
        public required Exception? error { get; set; }
    }
}