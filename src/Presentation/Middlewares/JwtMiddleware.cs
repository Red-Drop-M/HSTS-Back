using Application.Interfaces;
using System.Security.Claims;

namespace Presentation.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IJwtService _jwtService;

        public JwtMiddleware(RequestDelegate next, IJwtService jwtService)
        {
            _next = next;
            _jwtService = jwtService;
        }

        public async Task Invoke(HttpContext context)
        {
            // First try to get token from Authorization header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // If not found in header, try to get from cookie
            if (token == null)
            {
                token = context.Request.Cookies["auth_token"];
            }

            if (token != null)
            {
                AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, string token)
        {
            var userDto = _jwtService.GetUserFromToken(token);
            if (userDto != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
                    new Claim(ClaimTypes.Name, userDto.Name),
                    new Claim(ClaimTypes.Email, userDto.Email),
                    new Claim(ClaimTypes.Role, userDto.Role)
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                context.User = new ClaimsPrincipal(identity);
            }
        }
    }
}