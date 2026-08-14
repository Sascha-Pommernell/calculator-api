using System.Threading.RateLimiting;
using Calculator.Api.Infrastructure;
using Calculator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<ICalculatorService, CalculatorService>();

// Einheitliche Fehlerantworten (RFC 9457) – auch für unerwartete Exceptions und Statuscodes ohne Body.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CalculationExceptionHandler>();

builder.Services.AddHealthChecks();

// Schutz vor Überlastung: max. 200 Requests pro Sekunde und Client-IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unbekannt",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromSeconds(1),
            }));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Calculator API",
            Version = "v1",
            Description = "Eine REST-API für Grundrechenarten (Addition, Subtraktion, Multiplikation, Division) mit zwei oder mehr Zahlen."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("../openapi/v1.json", "Calculator API v1");
    });
}

app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
