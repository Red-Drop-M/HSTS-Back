using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Queries;
using Domain.ValueObjects;
using Shared.Exceptions;

namespace Presentation.Endpoints.GlobalStock
{
    public class GetGlobalStock : Endpoint<GetGlobalStockRequest, GetGlobalStockResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetGlobalStock> _logger;

        public GetGlobalStock(IMediator mediator, ILogger<GetGlobalStock> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/global-stocks/by-key");
            AllowAnonymous();
            Description(x => x
                .WithName("GetGlobalStock")
                .Produces<GetGlobalStockResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetGlobalStockRequest req, CancellationToken ct)
        {
            try
            {
                var query = new GetGlobalStockByKeyQuery(
                    BloodType.FromString(req.BloodType),
                    BloodBagType.Convert(req.BloodBagType));

                var result = await _mediator.Send(query, ct);

                if (result.err != null)
                {
                    _logger.LogError("GetGlobalStockByKeyHandler returned error: {Error}", result.err.Message);
                    throw result.err;
                }

                if (result.globalStock == null)
                {
                    _logger.LogWarning("Global stock not found for {BloodType} and {BloodBagType}", req.BloodType, req.BloodBagType);
                    throw new NotFoundException("Global stock not found", "get_global_stock");
                }

                _logger.LogInformation("Retrieved global stock for {BloodType} and {BloodBagType}", req.BloodType, req.BloodBagType);

                var response = new GetGlobalStockResponse
                {
                    GlobalStock = result.globalStock,
                    Message = "Global stock retrieved successfully",
                    StatusCode = StatusCodes.Status200OK
                };

                await SendAsync(response, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGlobalStock endpoint");
                throw;
            }
        }
    }

    public class GetGlobalStockRequest
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
    }

    public class GetGlobalStockResponse
    {
        public GlobalStockDTO? GlobalStock { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}