using FastEndpoints;
using MediatR;
using Application.Features.ServiceManagement.Queries;
using Domain.Entities;
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
    public override async Task HandleAsync(GetServiceByIdQuery req, CancellationToken ct)
    {
        var service = await _sender.Send(req, ct);
        if (service is null)
            await SendNotFoundAsync(ct);
        else
            await SendOkAsync(service, ct);
    }
}
