using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Logging;
using Application.Features.Users.Queries;
using Application.DTOs;
using Shared.Exceptions;

namespace Presentation.Endpoints.Admin.Users
{
    public class GetUsers : EndpointWithoutRequest<GetUsersResponse>
    {
        private readonly ILogger<GetUsers> _logger;
        private readonly IMediator _mediator;

        public GetUsers(ILogger<GetUsers> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/admin/users");
            Policies("RequireAdminRole"); // Restrict to admin role
            Description(x => x
                .WithName("GetUsers")
                .WithTags("Admin", "Users")
                .Produces<GetUsersResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status500InternalServerError));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                var query = new GetUsersQuery();
                var (users, error) = await _mediator.Send(query, ct);

                if (error != null)
                {
                    _logger.LogWarning("Failed to retrieve users: {Message}", error.Message);
                    throw error;
                }

                _logger.LogInformation("Retrieved {Count} users", users.Count);
                
                await SendAsync(new GetUsersResponse
                {
                    Users = users.Select(u => new UserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        DateOfBirth = u.DateOfBirth
                    }).ToList(),
                    Success = true
                }, StatusCodes.Status200OK, ct);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                _logger.LogError(ex, "Error retrieving users");
                throw new InternalServerException("An error occurred while processing your request", "GetUsers");
            }
        }
    }

    public class GetUsersResponse
    {
        public List<UserDto> Users { get; set; } = new List<UserDto>();
        public bool Success { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}