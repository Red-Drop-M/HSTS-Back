using FastEndpoints;
using Application.Features.DonorManagement.Commands;
using Application.Common.Models;

namespace Presentation.Endpoints.DonorManagement
{
    public class UpdateDonorEndpoint : Endpoint<UpdateDonorCommand, Result<bool>>
    {
        private readonly ISender _sender;

        public UpdateDonorEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Put("/donors/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateDonorCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 