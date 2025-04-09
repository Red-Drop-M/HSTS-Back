using FastEndpoints;
using Application.Features.BloodBagManagement.Commands;
using Application.Common.Models;

namespace Presentation.Endpoints.BloodBagManagement
{
    public class DeleteBloodBagEndpoint : Endpoint<DeleteBloodBagCommand, Result<bool>>
    {
        private readonly ISender _sender;

        public DeleteBloodBagEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Delete("/bloodbags/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteBloodBagCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 