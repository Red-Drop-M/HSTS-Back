using FluentValidation;
using MediatR;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using HSTS_Back.Presentation.Middlewares;
using Infrastructure.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Infrastructure.DependencyInjection;
using Infrastructure.ExternalServices.Kafka;
using Application.interfaces;
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel explicitly
builder.WebHost.ConfigureKestrel(serverOptions => {
    serverOptions.ListenAnyIP(5000);
    serverOptions.AllowSynchronousIO = true;
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
          .EnableSensitiveDataLogging()
          .EnableDetailedErrors());
Console.WriteLine("Database connection string: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
               throw new InvalidOperationException("Connection string 'DefaultConnection' not found."), 
               name: "postgres");

// Register Kafka configuration
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

// Register KafkaEventPublisher as a singleton or scoped service
builder.Services.AddScoped<IEventProducer, KafkaEventPublisher>();

// FastEndpoints + Swagger
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddFastEndpoints();
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
app.UseCors("AllowAll");

// Basic health check endpoint for troubleshooting
app.MapGet("/ping", () => "pong");
app.MapHealthChecks("/health");

// Use FastEndpoints + Swagger
app.UseFastEndpoints(c => {
    c.Endpoints.RoutePrefix = "api";
});
app.UseSwaggerGen(); // FastEndpoints Swagger

Console.WriteLine("Starting web server on port 5000...");

app.Run();