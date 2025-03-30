var builder = WebApplication.CreateBuilder(args);

// Set Kestrel to listen on specific ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5008);  // HTTP
    options.ListenAnyIP(7016, listenOptions => listenOptions.UseHttps());  // HTTPS
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection(); // Commented out to disable HTTPS redirection
    app.UseHsts();
}

// Map routes
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.MapGet("/weatherforecast2", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast2")
.WithOpenApi();


app.MapGet("/", () => {
    Console.WriteLine("Root route hit!");
    return "Hello World!";
})
.WithName("GetHelloWorld")
.WithOpenApi()
.Produces<string>(StatusCodes.Status200OK);

app.MapGet("/test", () => "Test Route")
   .WithName("GetTestRoute")
   .WithOpenApi()
   .Produces<string>(StatusCodes.Status200OK);



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
