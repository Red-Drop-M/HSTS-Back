using FastEndpoints;
using Application.Features.BloodBagManagement.Commands;
using Application.Common.Models;
using MediatR;

namespace Presentation.Endpoints.BloodBagManagement
{
    public class CreateBloodBagEndpoint : Endpoint<CreateBloodBagCommand, Result<Guid>>
    {
        private readonly ISender _sender;

        public CreateBloodBagEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Post("/bloodbags");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateBloodBagCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 