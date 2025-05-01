using FluentValidation;
using MediatR;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using HSTS_Back.Presentation.Middlewares;
using Infrastructure.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
Console.WriteLine("Database connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// FastEndpoints + Swagger
builder.Services.AddInfrastructureServices();
builder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true);
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "HSTS API";
        s.Version = "v1";
        s.Description = "API for HSTS project";
    };
});

// MediatR
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Use FastEndpoints + Swagger

app.UseFastEndpoints();
app.UseSwaggerGen(); // FastEndpoints Swagger

app.Run();
