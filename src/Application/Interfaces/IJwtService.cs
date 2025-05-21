using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        AuthUserDTO? GetUserFromToken(string token);
    }
}