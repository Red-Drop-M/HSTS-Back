using Application.DTOs;
using Application.Features.Authentication.Commands;
using Application.Interfaces;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;

namespace Application.Features.Authentication.Handlers
{
    public class LoginHandler : IRequestHandler<LoginCommand, (AuthResponseDTO? Response, BaseException? Error)>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<LoginHandler> _logger;

        public LoginHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<LoginHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<(AuthResponseDTO? Response, BaseException? Error)> Handle(
            LoginCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);
                var user = await _userRepository.GetByEmailAsync(request.Email);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", request.Email);
                    return (null, new UnauthorizedException("Invalid email or password", "Login"));
                }

                // Direct password comparison - in a real app, use proper password hashing
                if (user.Password != request.Password)
                {
                    _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                    return (null, new UnauthorizedException("Invalid email or password", "Login"));
                }

                // Generate JWT token
                _logger.LogInformation("Generating token for user: {Email}", request.Email);
                var token = _jwtService.GenerateToken(user);

                var response = new AuthResponseDTO
                {
                    Token = token,
                    User = new AuthUserDTO
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.Role
                    }
                };

                _logger.LogInformation("User {Email} authenticated successfully", request.Email);
                return (response, null);
            }
            catch (BaseException ex)
            {
                _logger.LogError(ex, "Error during login");
                return (null, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return (null, new InternalServerException("An error occurred while processing your request", "Login"));
            }
        }
    }
}