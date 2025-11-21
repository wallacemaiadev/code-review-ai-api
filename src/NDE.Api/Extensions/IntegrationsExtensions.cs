using NDE.Application.Integrations.Azure;

namespace NDE.Api.Extensions;

public static class IntegrationsExtensions
{
  public static void AddIntegrationsService(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IAzureIntegration, AzureIntegration>();
  }
}