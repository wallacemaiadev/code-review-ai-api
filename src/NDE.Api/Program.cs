using NDE.Api.Extensions;
using NDE.Api.Middleware;
using NDE.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args).AddObservability();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApiVersioningService();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPersistenceService(builder.Configuration);
builder.Services.AddQdrantService(builder.Configuration);
builder.Services.AddIntegrationsService(builder.Configuration);

var app = builder.Build();

await app.ApplyMigrationsAsync();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
