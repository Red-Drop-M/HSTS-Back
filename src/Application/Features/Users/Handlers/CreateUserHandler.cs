using MediatR;
using Domain.Entities;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.Users.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Handlers
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, (UserDTO? user, BaseException? error)>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<CreateUserHandler> _logger;

        public CreateUserHandler(IUserRepository userRepository, ILogger<CreateUserHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(UserDTO? user, BaseException? error)> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating user with email: {Email}", command.Email);
                
                // Check if email already exists
                var existingUser = await _userRepository.GetByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return (null, new BadRequestException($"User with email {command.Email} already exists", "CreateUser"));
                }

                var user = new User(
                    command.Name,
                    command.Email,
                    command.Password, // In a real app, would hash the password here
                    command.Role,
                    command.DateOfBirth.ToUniversalTime(), // Make sure DateTime is UTC
                    command.PhoneNumber,
                    command.Address
                );

                await _userRepository.AddAsync(user);

                _logger.LogInformation("User created successfully with ID: {Id}", user.Id);

                return (new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.Role,
                    DateOfBirth = user.DateOfBirth,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address
                }, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error creating user");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return (null, new InternalServerException("An error occurred while creating the user", "CreateUser"));
            }
        }
    }
}