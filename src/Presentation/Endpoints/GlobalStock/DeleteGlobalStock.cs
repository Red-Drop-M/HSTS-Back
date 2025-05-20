using FastEndpoints;
using MediatR;
using Application.Features.GlobalStockManagement.Commands;
using Domain.ValueObjects;
using Shared.Exceptions;

namespace Presentation.Endpoints.GlobalStock
{
    public class DeleteGlobalStock : Endpoint<DeleteGlobalStockRequest, DeleteGlobalStockResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteGlobalStock> _logger;

        public DeleteGlobalStock(IMediator mediator, ILogger<DeleteGlobalStock> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Delete("/global-stocks");
            AllowAnonymous();
            Description(x => x
                .WithName("DeleteGlobalStock")
                .Produces<DeleteGlobalStockResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(DeleteGlobalStockRequest req, CancellationToken ct)
        {
            try
            {
                var command = new DeleteGlobalStockCommand(
                    BloodType.FromString(req.BloodType),
                    BloodBagType.Convert(req.BloodBagType));

                var result = await _mediator.Send(command, ct);

                if (result != null)
                {
                    _logger.LogError("DeleteGlobalStockHandler returned error: {Error}", result.Message);
                    throw result;
                }

                _logger.LogInformation("Global stock deleted for {BloodType} and {BloodBagType}", req.BloodType, req.BloodBagType);

                var response = new DeleteGlobalStockResponse
                {
                    Message = "Global stock deleted successfully",
                    StatusCode = StatusCodes.Status200OK
                };

                await SendAsync(response, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteGlobalStock endpoint");
                throw;
            }
        }
    }

    public class DeleteGlobalStockRequest
    {
        public required string BloodType { get; set; }
        public required string BloodBagType { get; set; }
    }

    public class DeleteGlobalStockResponse
    {
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}