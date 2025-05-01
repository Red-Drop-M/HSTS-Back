using FastEndpoints;
using Application.DTOs;
using MediatR;
using Application.Features.BloodRequests.Queries;

namespace Presentation.Endpoints.BloodRequests
{
    public class GetRequest : Endpoint<GetRequestRequest,GetRequestResponse>
    {
        private readonly ILogger<GetRequest> _logger;
        private readonly IMediator _mediator;

        public GetRequest(ILogger<GetRequest> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/bloodrequests/{id}");
            AllowAnonymous();
            Description(x => x
                .Produces<GetRequestResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(GetRequestRequest req, CancellationToken ct)
        {
                var Query= new GetRequestQuery(req.Id);
                var (result,err) = await _mediator.Send(Query, ct);

                if (err != null)
                {
                    _logger.LogError("GetRequestHandler returned error: {Error}", err);
                    throw err;
                }

                await SendAsync(new GetRequestResponse(result,200,"request fetched succefully"), cancellation: ct);
            }

        }
    }
    public class GetRequestRequest
    {
        public Guid Id { get; set; }
    }
    public class GetRequestResponse
    {
        public RequestDto Request { get; set; }
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string Message { get; set; } = "Request retrieved successfully";
        public GetRequestResponse(RequestDto request, int statusCode, string message)
        {
            Request = request;
            StatusCode = statusCode;
            Message = message;
        }
}

