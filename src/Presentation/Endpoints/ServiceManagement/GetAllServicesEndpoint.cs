using FastEndpoints;
using MediatR;
using Application.Features.ServiceManagement.Queries;
using Domain.Entities;

public class GetAllServicesEndpoint : EndpointWithoutRequest<List<Service?>>
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
        var result = await _sender.Send(new GetAllServicesQuery(), ct);
        await SendOkAsync(result.Value, ct);
    }
}
