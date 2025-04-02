using System.Net;
using System.Text.Json;
using Shared.Exceptions;

namespace Presentation.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BaseException ex) // Custom Exceptions
            {
                _logger.LogError(ex, "Handled exception");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex.StatusCode;

                var response = new { message = ex.Message, resource = ex.Resource, statusCode = ex.StatusCode };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex) // Unhandled Exceptions
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new { message = "Internal Server Error", statusCode = 500 };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
