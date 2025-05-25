using FastEndpoints;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Shared.Exceptions;
using System;

namespace Presentation.Endpoints.Notifications
{
    public class GetPublicKey : EndpointWithoutRequest<GetPublicKeyResponse>
    {
        private readonly IWebPushService _webPushService;
        private readonly ILogger<GetPublicKey> _logger;

        public GetPublicKey(IWebPushService webPushService, ILogger<GetPublicKey> logger)
        {
            _webPushService = webPushService;
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/notifications/vapid-public-key");
            AllowAnonymous(); // Allow unauthenticated access
            Description(x => x
                .WithName("GetVapidPublicKey")
                .WithTags("Notifications")
                .Produces<GetPublicKeyResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var publicKey = await _webPushService.GetPublicKeyAsync();
                
                await SendAsync(new GetPublicKeyResponse
                {
                    PublicKey = publicKey
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving VAPID public key");
                throw new InternalServerException("Failed to retrieve VAPID public key", "get-vapid-key");
            }
        }
    }

    public class GetPublicKeyResponse
    {
        public string PublicKey { get; set; } = string.Empty;
    }
}