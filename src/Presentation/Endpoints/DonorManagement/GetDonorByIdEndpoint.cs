using FastEndpoints;
using Application.Features.DonorManagement.Queries;
using Application.Common.Models;
using Domain.Entities;

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
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 