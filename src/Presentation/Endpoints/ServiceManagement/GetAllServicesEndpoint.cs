using FastEndpoints;
using Application.Features.ServiceManagement.Command;
using Application.Common.Models;

public class GetAllServicesEndpoint : EndpointWithoutRequest<List<ServiceResponse>>
{
    private readonly ISender _sender;

    public GetAllServicesEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Get("/services");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var services = await _sender.Send(new GetAllServicesQuery(), ct);
        await SendOkAsync(services, ct);
    }
}
