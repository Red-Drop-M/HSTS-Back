using FastEndpoints;
using Application.Features.BloodBagManagement.Queries;
using Application.Common.Models;
using Domain.Entities;

namespace Presentation.Endpoints.BloodBagManagement
{
    public class GetAllBloodBagsEndpoint : Endpoint<GetAllBloodBagsQuery, Result<List<BloodBag?>>>
    {
        private readonly ISender _sender;

        public GetAllBloodBagsEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Get("/bloodbags");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetAllBloodBagsQuery req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 