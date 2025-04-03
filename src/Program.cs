using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HSTS_Back.Presentation.Middlewares;
using System;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
Console.WriteLine($"Connection String: {connectionString}");
// Register MediatR
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add Controllers
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "An example API using MediatR, FluentValidation, and Swagger"
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>(); // Register the custom exception middleware

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();

// HSTS-Back configuration
var hstsBackConfig = new
{
    commandName = "Project",
    dotnetRunMessages = true,
    applicationUrl = "http://localhost:5000;https://localhost:5001",
    environmentVariables = new
    {
        ASPNETCORE_ENVIRONMENT = "Development"
    }
};
