using FastEndpoints;
using Application.Features.ServiceManagement.Command;
using Application.Common.Models;

public class UpdateServiceEndpoint : Endpoint<UpdateServiceCommand>
{
    private readonly ISender _sender;

    public UpdateServiceEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Put("/services/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateServiceCommand req, CancellationToken ct)
    {
        await _sender.Send(req, ct);
        await SendNoContentAsync(ct);
    }
}
