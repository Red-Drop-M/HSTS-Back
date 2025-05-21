using MediatR;
using FastEndpoints;
using Domain.ValueObjects;
using Application.Features.DonorManagement.Commands;
using Shared.Exceptions;
using Application.DTOs;

namespace Presentation.Endpoints.Donor
{
    public class CreateDonor : Endpoint<CreateDonorRequest, CreateDonorResponse>
    {
        private readonly ILogger<CreateDonor> _logger;
        private readonly IMediator _mediator;

        public CreateDonor(ILogger<CreateDonor> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/donors");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("CreateDonor")
                .WithTags("Donors")
                .Produces<CreateDonorResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateDonorRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateDonorCommand(
                    req.Name,
                    req.Email,
                    req.DateOfBirth,
                    BloodType.FromString(req.BloodType),
                    req.Address,
                    req.NIN,
                    req.PhoneNumber,
                    req.LastDonationDate
                );

                var result = await _mediator.Send(command, ct);
                if (result.donor == null )
                {
                    _logger.LogError("CreateDonorHandler returned null");
                    throw new InvalidOperationException("CreateDonorHandler returned null: CreateDonor");
                }

                _logger.LogInformation("CreateDonorHandler success returned {result}", result);
                var response = new CreateDonorResponse
                {
                    content = result.donor,
                    success = true,
                    Error = null
                };
                await SendAsync(response, cancellation: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donor: {Message}", ex.Message);
                throw ex;
            }
        }
        }
    public class CreateDonorRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string BloodType { get; set; } 
        public DateOnly? LastDonationDate { get; set; } 
        public required string Address { get; set; }
        public required string NIN { get; set; }
        public required string PhoneNumber { get; set; }
        public required DateOnly DateOfBirth { get; set; } // DateOfBirth as string for input
    }
    public class CreateDonorResponse
    {
        public DonorDTO? content { get; set; }
        public bool? success { get; set; }
        public string? Error { get; set; }

    }        
}