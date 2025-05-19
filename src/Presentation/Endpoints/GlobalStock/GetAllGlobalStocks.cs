using FastEndpoints;
using MediatR;
using Application.DTOs;
using Application.Features.GlobalStockManagement.Queries;
using Shared.Exceptions;

namespace Presentation.Endpoints.GlobalStock
{
    public class GetAllGlobalStocks : Endpoint<GetAllGlobalStocksRequest, GetAllGlobalStocksResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAllGlobalStocks> _logger;

        public GetAllGlobalStocks(IMediator mediator, ILogger<GetAllGlobalStocks> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/global-stocks");
            AllowAnonymous();
            Description(x => x
                .WithName("GetAllGlobalStocks")
                .Produces<GetAllGlobalStocksResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetAllGlobalStocksRequest req, CancellationToken ct)
        {
            try
            {
                var query = new GetAllGlobalStocksQuery(
                    req.BloodType,
                    req.BloodBagType,
                    req.Critical);

                var result = await _mediator.Send(query, ct);

                if (result.err != null)
                {
                    _logger.LogError("GetAllGlobalStocksHandler returned error: {Error}", result.err.Message);
                    throw result.err;
                }

                if (result.stocks == null)
                {
                    _logger.LogWarning("No global stocks found");
                    throw new NotFoundException("No global stocks found", "get_all_global_stocks");
                }

                _logger.LogInformation("Retrieved {Count} global stocks", result.stocks.Count);

                var response = new GetAllGlobalStocksResponse
                {
                    GlobalStocks = result.stocks,
                    Total = result.total ?? 0,
                    Message = "Global stocks retrieved successfully",
                    StatusCode = StatusCodes.Status200OK
                };

                await SendAsync(response, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllGlobalStocks endpoint");
                throw;
            }
        }
    }

    public class GetAllGlobalStocksRequest
    {
        public string? BloodType { get; set; }
        public string? BloodBagType { get; set; }
        public bool? Critical { get; set; }
    }

    public class GetAllGlobalStocksResponse
    {
        public List<GlobalStockDTO>? GlobalStocks { get; set; }
        public int Total { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
    }
}