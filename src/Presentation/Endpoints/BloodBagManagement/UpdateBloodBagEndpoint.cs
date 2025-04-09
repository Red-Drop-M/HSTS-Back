using FastEndpoints;
using Application.Features.BloodBagManagement.Commands;
using Application.Common.Models;

namespace Presentation.Endpoints.BloodBagManagement
{
    public class UpdateBloodBagEndpoint : Endpoint<UpdateBloodBagCommand, Result<bool>>
    {
        private readonly ISender _sender;

        public UpdateBloodBagEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Put("/bloodbags/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateBloodBagCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 