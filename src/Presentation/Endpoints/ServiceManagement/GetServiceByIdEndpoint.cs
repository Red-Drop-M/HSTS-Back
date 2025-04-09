using FastEndpoints;
using Application.Features.ServiceManagement.Command;
using Application.Common.Models;

public class GetServiceByIdEndpoint : Endpoint<GetServiceByIdQuery, Result<Service?>>
{
    private readonly ISender _sender;

    public GetServiceByIdEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/services/{id}");
        AllowAnonymous();
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var service = await _sender.Send(new GetServiceByIdQuery(id), ct);
        if (service is null)
            await SendNotFoundAsync(ct);
        else
            await SendOkAsync(service, ct);
    }
}
