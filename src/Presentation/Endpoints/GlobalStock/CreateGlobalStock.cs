using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Domain.ValueObjects;
using Shared.Exceptions;

namespace Presentation.Endpoints.GlobalStock
{
    public class CreateGlobalStock : Endpoint<CreateGlobalStockRequest, CreateGlobalStockResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateGlobalStock> _logger;

        public CreateGlobalStock(IMediator mediator, ILogger<CreateGlobalStock> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/global-stocks");
            AllowAnonymous();
            Description(x => x
                .WithName("CreateGlobalStock")
                .Produces<CreateGlobalStockResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateGlobalStockRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateGlobalStockCommand(
                    BloodType.FromString(req.BloodType),
                    BloodBagType.Convert(req.BloodBagType),
                    req.CountExpired,
                    req.CountExpiring,
                    req.ReadyCount,
                    req.MinStock,
                    req.CriticalStock);

                var result = await _mediator.Send(command, ct);

                if (result.err != null)
                {
                    _logger.LogError("CreateGlobalStockHandler returned error: {Error}", result.err.Message);
                    throw result.err;
                }

                if (result.globalStock == null)
                {
                    _logger.LogError("CreateGlobalStockHandler returned null");
                    throw new InternalServerException("Failed to create global stock", "create_global_stock");
                }

                _logger.LogInformation("Global stock created for {BloodType} and {BloodBagType}", req.BloodType, req.BloodBagType);

                var response = new CreateGlobalStockResponse
                {
                    GlobalStock = result.globalStock,
                    Message = "Global stock created successfully",
                    StatusCode = StatusCodes.Status201Created
                };

                await SendAsync(response, StatusCodes.Status201Created, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateGlobalStock endpoint");
                throw;
            }
        }
    }

    public class CreateGlobalStockRequest
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
        public required int CountExpired { get; set; }
        public required int CountExpiring { get; set; }
        public required int ReadyCount { get; set; }
        public required int MinStock { get; set; }
        public required int CriticalStock { get; set; }
    }

    public class CreateGlobalStockResponse
    {
        public GlobalStockDTO? GlobalStock { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}