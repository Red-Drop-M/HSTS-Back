using FastEndpoints;
using Application.Features.DonorManagement.Commands;
using Application.Common.Models;

namespace Presentation.Endpoints.DonorManagement
{
    public class DeleteDonorEndpoint : Endpoint<DeleteDonorCommand, Result<bool>>
    {
        private readonly ISender _sender;

        public DeleteDonorEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Delete("/donors/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteDonorCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 