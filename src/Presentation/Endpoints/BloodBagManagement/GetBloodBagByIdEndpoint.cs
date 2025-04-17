using FastEndpoints;
using Application.Features.BloodBagManagement.Queries;
using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Presentation.Endpoints.BloodBagManagement
{
    public class GetBloodBagByIdEndpoint : Endpoint<GetBloodBagByIdQuery, Result<BloodBag?>>
    {
        private readonly ISender _sender;

        public GetBloodBagByIdEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Get("/bloodbags/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetBloodBagByIdQuery req, CancellationToken ct)
        {
            
            var result = await _sender.Send(req, ct);
            if (result == null)
            {
                await SendNotFoundAsync(ct);
            }else
            {
                await SendOkAsync(result, ct);
            }
        }
    }
} 