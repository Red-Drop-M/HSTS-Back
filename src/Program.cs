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
using Application.Interfaces;
using Application.Features.EventHandling.Commands;
using Infrastructure.BackgroundServices;
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
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") ?? 
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found."), 
        name: "postgres");

// Register Kafka configuration
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

// Register singleton for topic initialization
builder.Services.AddSingleton<KafkaTopicInitializer>();

// Register scoped services
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
builder.Services.AddHostedService<KafkaConsumerService>();

// Add background processing service
builder.Services.AddHostedService<BackgroundEventProcessor>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// Register the DailySchedulerService
builder.Services.AddHostedService<HSTS_Back.Infrastructure.BackgroundServices.DailySchedulerService>();

var app = builder.Build();

// Move logging middleware to be one of the first middleware components
// Custom request logging middleware
app.Use(async (context, next) => {
    var start = DateTime.UtcNow;
    var requestPath = context.Request.Path;
    var method = context.Request.Method;
    
    app.Logger.LogInformation("Request started: {Method} {Path}", method, requestPath);
    
    try {
        await next();
        
        var elapsed = DateTime.UtcNow - start;
        var statusCode = context.Response.StatusCode;
        
        app.Logger.LogInformation(
            "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
            method, requestPath, statusCode, elapsed.TotalMilliseconds);
    }
    catch (Exception ex) {
        app.Logger.LogError(ex, "Request failed: {Method} {Path}", method, requestPath);
        throw; // Re-throw to let the exception handler middleware handle it
    }
});

// Basic health check endpoint for troubleshooting
app.MapGet("/", () => "Hello from HSTS API!");
app.MapGet("/debug", (HttpContext context) => {
    return $"Debug info - Remote IP: {context.Connection.RemoteIpAddress}";
});
app.MapHealthChecks("/health");
app.MapGet("/ping", () => Results.Ok("pong"));
app.MapGet("/api/system/status", () => new { 
    Status = "Running", 
    SchedulerActive = true,
    CurrentTime = DateTime.Now 
});

// Add all other middleware
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseFastEndpoints();
app.UseSwaggerGen();

// Register your topic handlers with the dispatcher
var topicDispatcher = app.Services.GetRequiredService<ITopicDispatcher>();
topicDispatcher.Register<DonorPledgeCommand>("donors-pledges");

Console.WriteLine("Starting web server on port 5000...");
app.Logger.LogInformation("Web server is ready to accept requests at http://0.0.0.0:5000");

// In Program.cs after service registration
var serviceProvider = app.Services;
var topicInitializer = serviceProvider.GetRequiredService<KafkaTopicInitializer>();
await topicInitializer.InitializeAsync();

// Add a delay to ensure topics are ready
await Task.Delay(2000); 

app.Logger.LogInformation("Kafka topics initialized, starting application");

app.Run();