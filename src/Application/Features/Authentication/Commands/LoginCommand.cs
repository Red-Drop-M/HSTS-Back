using MediatR;
using Application.DTOs;
using Shared.Exceptions;

namespace Application.Features.Authentication.Commands
{
    public class LoginCommand : IRequest<(AuthResponseDTO? Response, BaseException? Error)>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}