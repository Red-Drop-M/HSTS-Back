using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Application.DTOs;
using Application.Features.Authentication.Commands;

namespace Presentation.Endpoints.Auth
{
    public class Login : Endpoint<LoginRequest, LoginResponse>
    {
        private readonly ILogger<Login> _logger;
        private readonly IMediator _mediator;

        public Login(ILogger<Login> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/auth/login");
            AllowAnonymous();
            Description(x => x
                .WithName("Login")
                .WithTags("Authentication")
                .Produces<LoginResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            try
            {
                var command = new LoginCommand
                {
                    Email = req.Email,
                    Password = req.Password
                };

                _logger.LogInformation("Attempting login for user: {Email}", req.Email);
                var (response, error) = await _mediator.Send(command, ct);

                if (error != null)
                {
                    _logger.LogWarning("Login failed for user {Email}: {Message}", req.Email, error.Message);
                    throw error;
                }

                if (response == null)
                {
                    _logger.LogError("LoginHandler returned null");
                    throw new InternalServerException("Authentication failed", "login");
                }

                // Set JWT token in HttpOnly cookie
                HttpContext.Response.Cookies.Append("auth_token", response.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // For HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7) // Match token expiration
                });

                _logger.LogInformation("User {Email} logged in successfully", req.Email);
                await SendAsync(new LoginResponse
                {
                    Token = response.Token, // Still include in response for API clients
                    User = new UserResponse
                    {
                        Id = response.User.Id,
                        Name = response.User.Name,
                        Email = response.User.Email,
                        Role = response.User.Role
                    },
                    Success = true
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error during login");
                throw new InternalServerException("An error occurred while processing your request", "login");
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = new();
        public bool Success { get; set; }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}