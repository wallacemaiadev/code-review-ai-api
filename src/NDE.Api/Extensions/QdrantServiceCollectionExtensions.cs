using NDE.VectorStore.Extensions;

namespace NDE.Api.Extensions;

public static class QdrantServiceCollectionExtensions
{
  public static IServiceCollection AddQdrantService(this IServiceCollection services, IConfiguration configuration)
  {
    if (services is null) throw new ArgumentNullException(nameof(services));
    if (configuration is null) throw new ArgumentNullException(nameof(configuration));


    var endpoint = configuration["VectorStore:Endpoint"];
    var apiKey = configuration["VectorStore:ApiKey"];

    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
      throw new InvalidOperationException("Configuração do qdrant está incorreta, verifique o appsettings.");

    services.AddQdrantVectorStore(endpoint, apiKey);

    return services;
  }
}
