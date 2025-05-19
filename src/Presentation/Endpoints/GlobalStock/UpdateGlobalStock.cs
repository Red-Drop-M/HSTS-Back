using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Commands;
using Domain.ValueObjects;
using Shared.Exceptions;

namespace Presentation.Endpoints.GlobalStock
{
    public class UpdateGlobalStock : Endpoint<UpdateGlobalStockRequest, UpdateGlobalStockResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateGlobalStock> _logger;

        public UpdateGlobalStock(IMediator mediator, ILogger<UpdateGlobalStock> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Put("/global-stocks");
            AllowAnonymous();
            Description(x => x
                .WithName("UpdateGlobalStock")
                .Produces<UpdateGlobalStockResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(UpdateGlobalStockRequest req, CancellationToken ct)
        {
            try
            {
                var command = new UpdateGlobalStockCommand(
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
                    _logger.LogError("UpdateGlobalStockHandler returned error: {Error}", result.err.Message);
                    throw result.err;
                }

                if (result.globalStock == null)
                {
                    _logger.LogError("UpdateGlobalStockHandler returned null");
                    throw new InternalServerException("Failed to update global stock", "update_global_stock");
                }

                _logger.LogInformation("Global stock updated for {BloodType} and {BloodBagType}", req.BloodType, req.BloodBagType);

                var response = new UpdateGlobalStockResponse
                {
                    GlobalStock = result.globalStock,
                    Message = "Global stock updated successfully",
                    StatusCode = StatusCodes.Status200OK
                };

                await SendAsync(response, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateGlobalStock endpoint");
                throw;
            }
        }
    }

    public class UpdateGlobalStockRequest
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
        public int? CountExpired { get; set; }
        public int? CountExpiring { get; set; }
        public int? ReadyCount { get; set; }
        public int? MinStock { get; set; }
        public int? CriticalStock { get; set; }
    }

    public class UpdateGlobalStockResponse
    {
        public GlobalStockDTO? GlobalStock { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}