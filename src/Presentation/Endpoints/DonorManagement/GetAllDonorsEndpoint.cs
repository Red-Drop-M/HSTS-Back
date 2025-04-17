using FastEndpoints;
using Application.Features.DonorManagement.Queries;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Presentation.Endpoints.DonorManagement
{
    public class GetAllDonorsEndpoint : Endpoint<GetAllDonorsQuery, Result<List<Donor?>>>
    {
        private readonly ISender _sender;

        public GetAllDonorsEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Get("/donors");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetAllDonorsQuery req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 