using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.Users.Commands;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Handlers
{
    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, (UserDTO? user, BaseException? error)>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserHandler> _logger;

        public UpdateUserHandler(IUserRepository userRepository, ILogger<UpdateUserHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(UserDTO? user, BaseException? error)> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating user with ID: {Id}", command.Id);
                
                var user = await _userRepository.GetByIdAsync(command.Id);
                if (user == null)
                {
                    return (null, new NotFoundException($"User with ID {command.Id} not found", "UpdateUser"));
                }

                // Update user details
                user.UpdateDetails(
                    command.Name,
                    command.Email,
                    command.PhoneNumber,
                    command.Address,
                    command.DateOfBirth?.ToUniversalTime() // Make sure DateTime is UTC
                );

                // Update password if provided
                if (!string.IsNullOrEmpty(command.Password))
                {
                    user.ChangePassword(command.Password); // In a real app, would hash the password
                }

                // Update role if provided
                if (command.Role != null)
                {
                    user.UpdateRole(command.Role);
                }

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("User {Id} updated successfully", user.Id);

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
                _logger.LogError(ex, "Error updating user {Id}", command.Id);
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", command.Id);
                return (null, new InternalServerException("An error occurred while updating the user", "UpdateUser"));
            }
        }
    }
}