using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Users.Commands;
using Application.DTOs;
using Shared.Exceptions;
using Domain.ValueObjects;

namespace Presentation.Endpoints.Admin.Users
{
    public class UpdateUser : Endpoint<UpdateUserRequest, UpdateUserResponse>
    {
        private readonly ILogger<UpdateUser> _logger;
        private readonly IMediator _mediator;

        public UpdateUser(ILogger<UpdateUser> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Put("/admin/users/{id}");
            Policies("RequireAdminRole"); // Restrict to admin role
            Description(x => x
                .WithName("UpdateUser")
                .WithTags("Admin", "Users")
                .Produces<UpdateUserResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            try
            {
                var userRole = req.Role != null ? UserRole.Convert(req.Role) : null;

                var command = new UpdateUserCommand(
                    req.Id,
                    req.Name,
                    req.Email,
                    req.Password,
                    userRole,
                    req.DateOfBirth,
                    req.PhoneNumber,
                    req.Address
                );

                var (user, error) = await _mediator.Send(command, ct);

                if (error != null)
                {
                    _logger.LogWarning("Failed to update user {Id}: {Message}", req.Id, error.Message);
                    ThrowError(error);
                    return;
                }

                if (user == null)
                {
                    throw new NotFoundException($"User with ID {req.Id} not found", "UpdateUser");
                }

                _logger.LogInformation("User {Id} updated successfully", req.Id);
                
                await SendAsync(new UpdateUserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    Success = true
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error updating user {Id}", req.Id);
                throw new InternalServerException("An error occurred while processing your request", "UpdateUser");
            }
        }
    }

    public class UpdateUserRequest
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateUserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}