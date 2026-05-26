using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductManagement.API.HealthChecks;
using ProductManagement.API.Middleware;
using ProductManagement.Application;
using ProductManagement.Infrastructure;
using ProductManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "Product Management API";
    options.Version = "v1";
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running."), tags: ["liveness"])
    .AddDbContextCheck<ProductManagementDbContext>("database", tags: ["readiness"]);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi(settings => settings.Path = "/swagger");
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse,
});

app.MapControllers();

app.Run();

public partial class Program;
