using FastEndpoints;
using Application.Features.DonorManagement.Commands;
using Application.Common.Models;

namespace Presentation.Endpoints.DonorManagement
{
    public class CreateDonorEndpoint : Endpoint<CreateDonorCommand, Result<Guid>>
    {
        private readonly ISender _sender;

        public CreateDonorEndpoint(ISender sender)
        {
            _sender = sender;
        }

        public override void Configure()
        {
            Post("/donors");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CreateDonorCommand req, CancellationToken ct)
        {
            var result = await _sender.Send(req, ct);
            await SendOkAsync(result, ct);
        }
    }
} 