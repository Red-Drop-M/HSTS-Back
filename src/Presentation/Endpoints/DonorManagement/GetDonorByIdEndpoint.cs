using FastEndpoints;
using Application.Features.DonorManagement.Queries;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Presentation.Endpoints.DonorManagement
{
    public class GetDonorByIdEndpoint : Endpoint<GetDonorByIdQuery, Result<Donor?>>
    {
        private readonly ISender _sender;

        public GetDonorByIdEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Get("/donors/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetDonorByIdQuery req, CancellationToken ct)
        {

            /*
             var id = Route<Guid>("id");
        var service = await _sender.Send(new GetServiceByIdQuery(id), ct);
        if (service is null)
            await SendNotFoundAsync(ct);
        else
            await SendOkAsync(service, ct);
            */
            var result = await _sender.Send(req, ct);
            if (result is null)
                await SendNotFoundAsync(ct);
            else
            await SendOkAsync(result, ct);
        }
    }
} 