using MediatR;
using Domain.Repositories;
using Application.DTOs;
using Application.Features.Users.Queries;
using Shared.Exceptions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Users.Handlers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, (List<UserDTO> users, BaseException? error)>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUsersHandler> _logger;

        public GetUsersHandler(IUserRepository userRepository, ILogger<GetUsersHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<(List<UserDTO> users, BaseException? error)> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting all users");
                
                var users = await _userRepository.GetUsersAsync();
                
                if (users == null || !users.Any())
                {
                    _logger.LogInformation("No users found");
                    return (new List<UserDTO>(), null);
                }

                var userDtos = users
                    .Where(u => u != null)
                    .Select(u => new UserDTO
                    {
                        Id = u!.Id,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role.Role,
                        DateOfBirth = u.DateOfBirth,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address
                    })
                    .ToList();

                _logger.LogInformation("Retrieved {Count} users", userDtos.Count);
                
                return (userDtos, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error getting users");
                return (new List<UserDTO>(), ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return (new List<UserDTO>(), new InternalServerException("An error occurred while retrieving users", "GetUsers"));
            }
        }
    }
}