using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Users.Commands;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Presentation.Endpoints.Admin.Users
{
    public class CreateUser : Endpoint<CreateUserRequest, CreateUserResponse>
    {
        private readonly ILogger<CreateUser> _logger;
        private readonly IMediator _mediator;

        public CreateUser(ILogger<CreateUser> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/admin/users");
            Policies("RequireAdminRole"); // Restrict to admin role
            Description(x => x
                .WithName("CreateUser")
                .WithTags("Admin", "Users")
                .Produces<CreateUserResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
        {
            try
            {
                var command = new CreateUserCommand(
                    req.Name,
                    req.Email,
                    req.Password,
                    UserRole.Convert(req.Role),
                    req.DateOfBirth,
                    req.PhoneNumber,
                    req.Address
                );

                var (user, error) = await _mediator.Send(command, ct);

                if (error != null)
                {
                    _logger.LogWarning("Failed to create user: {Message}", error.Message);
                    throw error;
                }

                if (user == null)
                {
                    throw new InternalServerException("Failed to create user", "CreateUser");
                }

                _logger.LogInformation("User created with ID: {UserId}", user.Id);
                
                await SendAsync(new CreateUserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Success = true
                }, StatusCodes.Status201Created, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error creating user");
                throw new InternalServerException("An error occurred while processing your request", "CreateUser");
            }
        }
    }

    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Default to regular user
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class CreateUserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}