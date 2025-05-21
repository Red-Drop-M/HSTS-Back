using MediatR;
using FastEndpoints;
using Application.Features.BloodRequests.Commands;
using Application.DTOs;
using Domain.ValueObjects;
namespace Presentation.Endpoints.BloodRequests
{
    public class UpdateRequest : Endpoint<UpdateRequestRequest,UpdateRequestResponse>
    {
        private readonly ILogger<UpdateRequest> _logger;
        private readonly IMediator _mediator;
        public UpdateRequest(ILogger<UpdateRequest> logger,IMediator mediator)
        {
            _logger=logger;
            _mediator= mediator;
        }
        public override void Configure()
        {
            Put("/bloodrequests/{id}");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("UpdateBloodRequest")
                .WithTags("BloodRequests")
                .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status200OK));
        }
        public override async Task HandleAsync(UpdateRequestRequest req, CancellationToken ct)
        {
            var Command = new UpdateRequestCommand(req.Id,req.BloodBagType ==null?BloodBagType.Convert(req.BloodBagType):null,req.Priority==null?Priority.Convert(req.Priority):null,req.DueDate,req.MoreDetails,req.RequiredQty);
            var (result,err) = await _mediator.Send(Command,ct);
            if(err != null)
            {
                _logger.LogError("Error while updating request");
                throw err;
            }
            _logger.LogInformation("request updated succeffuly");
            var response = new UpdateRequestResponse(result,200,"request updated succesfully");
            await SendAsync(response,cancellation:ct); 
        }  
    }
    public class UpdateRequestRequest{
        public required Guid Id {get;set;}
        public  string? BloodBagType { get; set; }
        public  DateOnly? DueDate { get; set; }
        public string? Priority {get;set;}
        public  string? MoreDetails { get; set; }
        public  DateOnly? RequestDate { get; set; }
        public  int RequiredQty { get; set; }
    } 
    public class UpdateRequestResponse
    {
        public int  status { get; set; }
        public string Message { get; set; }
        public RequestDto Request {get ; set;}
        public UpdateRequestResponse(RequestDto req,int s,string m)
        {
            Request = req;
            status = s;
            Message = m;
        }
    }
}