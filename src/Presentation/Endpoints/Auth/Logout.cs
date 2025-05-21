using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Presentation.Endpoints.Auth
{
    public class Logout : EndpointWithoutRequest<LogoutResponse>
    {
        private readonly ILogger<Logout> _logger;

        public Logout(ILogger<Logout> logger)
        {
            _logger = logger;
        }

        public override void Configure()
        {
            Post("/auth/logout");
            Roles("Admin", "User"); // Both admins and regular users can access
            Description(x => x
                .WithName("Logout")
                .WithTags("Authentication")
                .Produces<LogoutResponse>(StatusCodes.Status200OK));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            // Clear the auth cookie
            HttpContext.Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            _logger.LogInformation("User logged out");
            
            await SendAsync(new LogoutResponse
            {
                Success = true,
                Message = "Logged out successfully"
            }, cancellation: ct);
        }
    }

    public class LogoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}