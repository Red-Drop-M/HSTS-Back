using FastEndpoints;
using System.Security.Claims;
using Shared.Exceptions;

namespace Presentation.Endpoints.Auth
{
    public class GetCurrentUser : EndpointWithoutRequest<GetCurrentUserResponse>
    {
        private readonly ILogger<GetCurrentUser> _logger;

        public GetCurrentUser(ILogger<GetCurrentUser> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Get("/auth/me");
            Description(x => x
                .WithName("GetCurrentUser")
                .WithTags("Authentication")
                .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated");
                    throw new UnauthorizedException("User not authenticated", "get_current_user");
                }

                var response = new GetCurrentUserResponse
                {
                    Id = Guid.Parse(userId),
                    Name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                    Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                    Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                    IsAuthenticated = true,
                    Success = true,
                    Error = null
                };

                await SendAsync(response, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error retrieving current user");
                throw new InternalServerException("An error occurred while processing your request", "get_current_user");
            }
        }
    }

    public class GetCurrentUserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}