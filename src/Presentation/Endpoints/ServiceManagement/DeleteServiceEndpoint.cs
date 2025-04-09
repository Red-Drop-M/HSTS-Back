using FastEndpoints;
using Application.Features.ServiceManagement.Command;
using Application.Common.Models;

public class DeleteServiceEndpoint : EndpointWithoutRequest
{
    private readonly ISender _sender;

    public DeleteServiceEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Delete("/services/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await _sender.Send(new DeleteServiceCommand(id), ct);
        await SendNoContentAsync(ct);
    }
}
