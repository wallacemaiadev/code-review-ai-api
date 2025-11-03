using NDE.Application.Integrations.Azure;

namespace NDE.Api.Extensions;

public static class IntegrationsExtensions
{
  public static void AddIntegrationsService(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddHttpClient<IAzureIntegration, AzureIntegration>((sp, httpClient) =>
    {
      var azureToken = configuration["AzureToken"];
      if (string.IsNullOrWhiteSpace(azureToken))
        throw new InvalidOperationException("O Token do Azure DevOps não foi encontrado em 'AzureToken'.");

      var patBase64 = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{azureToken}"));
      httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", patBase64);
    });
  }
}