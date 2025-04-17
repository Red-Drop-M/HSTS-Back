using FastEndpoints;
using Application.Features.ServiceManagement.Commands;
using MediatR;
using Application.Common.Models;

public class CreateServiceEndpoint : Endpoint<CreateServiceCommand, Result<Guid>>
{
    private readonly ISender _sender;

    public CreateServiceEndpoint(ISender sender)
    {
        _sender = sender;
    }

    public override void Configure()
    {
        Post("/services");
        AllowAnonymous(); // TBD: RequireAuthorization() ???
    }

    public override async Task HandleAsync(CreateServiceCommand req, CancellationToken ct)
    {
        var id = await _sender.Send(req, ct);
        await SendCreatedAtAsync<GetServiceByIdEndpoint>(new { id }, id, generateAbsoluteUrl: true, cancellation: ct);
    }
}
